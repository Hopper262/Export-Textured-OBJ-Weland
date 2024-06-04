using Gtk;
using Weland;
using System;
using System.IO;
using System.Collections.Generic;

namespace Hopper {
  public class HopOBJExporter {
    const double Scale = 3.2;

    public HopOBJExporter(Level level) {
        this.level = level;
    }

    public void Export(string path) {
      faces.Clear();
      vertices.Clear();
      endpointVertices.Clear();
      for (int i = 0; i < level.Endpoints.Count; ++i) {
        endpointVertices.Add(new Dictionary<short, int>());
      }

      foreach (Polygon p in level.Polygons) {
        if (p.CeilingHeight > p.FloorHeight) {
          if (p.FloorTransferMode != 9) {
            faces.Add(FloorFace(p));
          }
          if (p.CeilingTransferMode != 9) {
            faces.Add(CeilingFace(p));
          }
          for (int i = 0; i < p.VertexCount; ++i) {
            InsertLineFaces(level.Lines[p.LineIndexes[i]], p);
          }
        }
      }
        
      string mtlpath;
      if (path.EndsWith(".obj")) {
        mtlpath = string.Concat(path.Substring(0, path.Length - 4), ".mtl");
      } else {
        mtlpath = string.Concat(path, ".mtl");
      }
        
      using (TextWriter mw = new StreamWriter(mtlpath)) {
        foreach (Material m in materials) {
          m.WriteNew(mw);
        }
      }
  
      using (TextWriter w = new StreamWriter(path)) {
        w.WriteLine("mtllib {0}", Path.GetFileName(mtlpath));
        w.WriteLine("s off");
        foreach (Vertex v in vertices) {
          v.Write(w);
        }
        
        foreach (TexCoord tc in texCoords) {
          tc.Write(w);
        }
        
        foreach (Face f in faces) {
          f.Write(w);
        }
      }
    }

    int GetTexCoordIndex(int U, int V) {
      TexCoord tc = new TexCoord();
      tc.U = U;
      tc.V = V;
      if (!texCoordsSeen.ContainsKey(tc)) {
        texCoords.Add(tc);
        texCoordsSeen[tc] = texCoords.Count;
      }
      return texCoordsSeen[tc];
    }
        
    int GetVertexIndex(int endpointIndex, short height) {
      if (!endpointVertices[endpointIndex].ContainsKey(height)) {
        Point p = level.Endpoints[endpointIndex];
        Vertex v = new Vertex();
        v.X = p.X;
        v.Y = p.Y;
        v.Z = height;
        vertices.Add(v);
        endpointVertices[endpointIndex][height] = vertices.Count;
      }
      return endpointVertices[endpointIndex][height];
    }
    
    int GetMaterialIndex(int coll, int bitmap, int light) {
      Material m = new Material();
      m.C = coll;
      m.B = bitmap;
      m.L = light;
      if (!materialsSeen.ContainsKey(m)) {
        materials.Add(m);
        materialsSeen[m] = materials.Count;
      }
      return materialsSeen[m];
    }
    Material GetMaterial(int coll, int bitmap, int light) {
      return materials[GetMaterialIndex(coll, bitmap, light) - 1];
    }

    Face FloorFace(Polygon p) {
      FaceCoord[] result = new FaceCoord[p.VertexCount];
      for (int i = 0; i < p.VertexCount; ++i) {
        int j = p.VertexCount - i - 1;
        int vi = GetVertexIndex(p.EndpointIndexes[i], p.FloorHeight);
        result[j].Vi = vi;
        Vertex v = vertices[vi - 1];
        result[j].Ti = GetTexCoordIndex(v.Y + p.FloorOrigin.Y, -(v.X + p.FloorOrigin.X));
      }
      Face f = new Face();
      f.C = result;
      f.M = GetMaterial(p.FloorTexture.Collection, p.FloorTexture.Bitmap, p.FloorLight);
      return f;
    }

    Face CeilingFace(Polygon p) {
      FaceCoord[] result = new FaceCoord[p.VertexCount];
      for (int i = 0; i < p.VertexCount; ++i) {
        int vi = GetVertexIndex(p.EndpointIndexes[i], p.CeilingHeight);
        result[i].Vi = vi;
        Vertex v = vertices[vi - 1];
        result[i].Ti = GetTexCoordIndex(v.Y + p.CeilingOrigin.Y, -(v.X + p.CeilingOrigin.X));
      }
      Face f = new Face();
      f.C = result;
      f.M = GetMaterial(p.CeilingTexture.Collection, p.CeilingTexture.Bitmap, p.CeilingLight);
      return f;
    }

    Face BuildFace(int left, int right, short floor, short ceiling,
                   Side.TextureDefinition texture, short light) {
      Point lp = level.Endpoints[left];
      Point rp = level.Endpoints[right];
      int length = (int)Math.Round(Math.Sqrt(Math.Pow(rp.X - lp.X, 2) +
                                        Math.Pow(rp.Y - lp.Y, 2)));
      int height = ceiling - floor;
      
      int xleft = texture.X;
      int xright = texture.X + length;
      int yceiling = 1024 - texture.Y;
      int yfloor = yceiling - height;
      
      FaceCoord[] result = new FaceCoord[4];
      result[0].Vi = GetVertexIndex(left, floor);
      result[0].Ti = GetTexCoordIndex(xleft, yfloor);
      result[1].Vi = GetVertexIndex(right, floor);
      result[1].Ti = GetTexCoordIndex(xright, yfloor);
      result[2].Vi = GetVertexIndex(right, ceiling);
      result[2].Ti = GetTexCoordIndex(xright, yceiling);
      result[3].Vi = GetVertexIndex(left, ceiling);
      result[3].Ti = GetTexCoordIndex(xleft, yceiling);
      
      Face f = new Face();
      f.C = result;
      f.M = GetMaterial(texture.Texture.Collection, texture.Texture.Bitmap, light);

      return f;
    }

    void InsertLineFaces(Line line, Polygon p) {
      int left;
      int right;
      Polygon opposite = null;
      Side side = null;
      if (line.ClockwisePolygonOwner != -1 && level.Polygons[line.ClockwisePolygonOwner] == p) {
        left = line.EndpointIndexes[0];
        right = line.EndpointIndexes[1];
        if (line.CounterclockwisePolygonOwner != -1) {
          opposite = level.Polygons[line.CounterclockwisePolygonOwner];
        }
        if (line.ClockwisePolygonSideIndex != -1) {
          side = level.Sides[line.ClockwisePolygonSideIndex];
        }
      } else {
        left = line.EndpointIndexes[1];
        right = line.EndpointIndexes[0];
        if (line.ClockwisePolygonOwner != -1) {
          opposite = level.Polygons[line.ClockwisePolygonOwner];
        }
        if (line.CounterclockwisePolygonSideIndex != -1) {
          side = level.Sides[line.CounterclockwisePolygonSideIndex];
        }
      }

      bool landscapeTop = false;
      bool landscapeBottom = false;
      if (side != null) {
        if (side.Type == Weland.SideType.Low) {
          if (side.PrimaryTransferMode == 9) {
              landscapeBottom = true;
          }
        } else {
          if (side.PrimaryTransferMode == 9) {
              landscapeTop = true;
          }
          if (side.SecondaryTransferMode == 9) {
              landscapeBottom = true;
          }
        }
      }

      if (opposite == null || (opposite.FloorHeight > p.CeilingHeight || opposite.CeilingHeight < p.FloorHeight)) {
        if (!landscapeTop) {
          faces.Add(BuildFace(left, right,
                              p.FloorHeight, p.CeilingHeight,
                              side.Primary,
                              side.PrimaryLightsourceIndex));
        }
      } else {
        if (opposite.FloorHeight > p.FloorHeight) {
          if (!landscapeBottom) {
            if (side.Type == Weland.SideType.Low) {
              faces.Add(BuildFace(left, right,
                                  p.FloorHeight, opposite.FloorHeight,
                                  side.Primary,
                                  side.PrimaryLightsourceIndex));
            } else {
              faces.Add(BuildFace(left, right,
                                  p.FloorHeight, opposite.FloorHeight,
                                  side.Secondary,
                                  side.SecondaryLightsourceIndex));
            }
          }
        }
        if (opposite.CeilingHeight < p.CeilingHeight) {
          if (!landscapeTop) {
            faces.Add(BuildFace(left, right,
                                opposite.CeilingHeight, p.CeilingHeight,
                                side.Primary,
                                side.PrimaryLightsourceIndex));
          }
        }
      }
    }

    struct Vertex {
      public short X;
      public short Y;
      public short Z;
      
      public void Write(TextWriter w) {
        w.WriteLine("v {0} {1} {2}", World.ToDouble(X) * Scale, World.ToDouble(Z) * Scale, World.ToDouble(Y) * Scale);
      }
    }

    struct TexCoord {
      public int U;
      public int V;
      
      public void Write(TextWriter w) {
        w.WriteLine("vt {0} {1}", World.ToDouble(U), World.ToDouble(V));
      }
    }

    struct Material {
      public int C;
      public int B;
      public int L;
      
      public void WriteNew(TextWriter w) {
        w.WriteLine("newmtl {0:D2}-{1:D3}-{2:D2}", C, B, L);
        
        double t1 = 0.05 * (double)(20 - L);
        if (L > 20) { t1 = 0; }
        double intensity = 1.0 - t1/2;
        w.WriteLine("Ka {0:F3} {0:F3} {0:F3}", intensity);
        w.WriteLine("Kd {0:F3} {0:F3} {0:F3}", intensity);
        w.WriteLine("Ks {0:F3} {0:F3} {0:F3}", 0.0);
        w.WriteLine("Ke {0:F3} {0:F3} {0:F3}", intensity);
        w.WriteLine("Ns {0:F3}", 0.0);
        w.WriteLine("Ni {0:F3}", 0.0);
        w.WriteLine("d {0:F3}", 1.0);
        w.WriteLine("Tr {0:F3}", 1.0);
        w.WriteLine("illum 2");
        w.WriteLine("map_Ka {0:D2}/bitmap{1:D3}.bmp", C, B);
        w.WriteLine("map_Kd {0:D2}/bitmap{1:D3}.bmp", C, B);
        w.WriteLine("map_Ke {0:D2}/bitmap{1:D3}.bmp", C, B);
      }
      
      public void WriteUse(TextWriter w) {
        w.WriteLine("usemtl {0:D2}-{1:D3}-{2:D2}", C, B, L);
      }
    }

    struct FaceCoord {
      public int Vi;
      public int Ti;
      
      public void Write(TextWriter w) {
        w.Write(" {0}/{1}", Vi, Ti);
      }
    }

    struct Face {
      public FaceCoord[] C;
      public Material M;
      
      public void Write(TextWriter w) {
        M.WriteUse(w);
        w.Write("f");
        foreach (FaceCoord fc in C) {
          fc.Write(w);
        }
        w.WriteLine();
      }
    }
      
    Level level;
    List<Vertex> vertices = new List<Vertex>();
    List<Dictionary<short, int>> endpointVertices = new List<Dictionary<short, int>>();
    List<Face> faces = new List<Face>();
    
    List<TexCoord> texCoords = new List<TexCoord>();
    Dictionary<TexCoord, int> texCoordsSeen = new Dictionary<TexCoord, int>();
    
    List<Material> materials = new List<Material>();
    Dictionary<Material, int> materialsSeen = new Dictionary<Material, int>();
  }
}
