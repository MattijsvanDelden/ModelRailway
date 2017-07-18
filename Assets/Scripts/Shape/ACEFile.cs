/*

// Type: MSTS.ACEFile
// Assembly: ACEFile, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 11418276-834D-46B7-97F4-D15FDE5E187C
// Assembly location: D:\Projects\C#\bin\ACEFile.dll

using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using System;
using System.IO;
using System.Text;
using UnityEngine;



namespace MSTS
{
  public class ACEFile
  {
    public static Texture2D Texture2DFromFile(string filename)
    {
      var fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
      var numArray1 = new byte[16];
      fileStream.Read(numArray1, 0, 16);
      BinaryReader fb = !(Encoding.ASCII.GetString(numArray1, 0, 8) == "SIMISA@F") ? new BinaryReader((Stream) fileStream) : new BinaryReader((Stream) new InflaterInputStream((Stream) fileStream));
      fb.ReadInt32();
      int num1 = fb.ReadInt32();
      int width = fb.ReadInt32();
      int height = fb.ReadInt32();
      int num2 = fb.ReadInt32();
      int length1 = fb.ReadInt32();
      int num3 = fb.ReadInt32();
      Encoding.ASCII.GetString(fb.ReadBytes(16), 0, 16);
      Encoding.ASCII.GetString(fb.ReadBytes(64), 0, 64);
      int num4 = fb.ReadByte();
      int num5 = fb.ReadByte();
      int num6 = fb.ReadByte();
      int num7 = fb.ReadByte();
      fb.ReadInt32();
      fb.ReadInt32();
      double num8 = fb.ReadSingle();
      fb.ReadInt32();
      fb.ReadInt32();
      fb.ReadBytes(20);
      var aceChannelArray = new ACEChannel[length1];
      for (int index = 0; index < length1; ++index)
        aceChannelArray[index] = new ACEFile.ACEChannel(fb);
      int num9 = 0;
      foreach (ACEChannel aceChannel in aceChannelArray)
      {
        switch (aceChannel.id)
        {
          case 2:
            if (aceChannel.nbits != 1)
              throw new Exception("Can't read ace file - expecting 1 bits in transparency channel but found " + (object) aceChannel.nbits);
            else
              break;
          case 3:
          case 4:
          case 5:
          case 6:
            if (aceChannel.nbits != 8)
              throw new Exception("Can't read ace file - expecting 8 bits in color channel but found " + (object) aceChannel.nbits);
            else
              break;
          default:
            if (aceChannel.nbits != 8 && aceChannel.nbits != 1)
              throw new Exception("Can't read ace file - can handle only 1 bit or 8 bit channels but found " + (object) aceChannel.nbits);
            else
              break;
        }
        num9 += aceChannel.nbits;
      }
      if (num3 != 0)
        throw new Exception("This file has palletes I don't know how to read them yet");
      int length2 = 0;
      if ((num1 & 1) == 1)
      {
        int num10 = width > height ? width : height;
        while (num10 > 0)
        {
          ++length2;
          num10 /= 2;
        }
      }
      else
        length2 = 1;
      bool flag = (num1 & 16) != 0;
      if (flag)
      {
        foreach (uint num10 in new uint[length2])
          num10 = fb.ReadUInt32();
      }
      else
      {
        int length3 = 0;
        int num10 = height;
        for (int index = 0; index < length2; ++index)
        {
          length3 += num10;
          num10 /= 2;
          if (num10 < 1)
            num10 = 1;
        }
        foreach (uint num11 in new uint[length3])
          num11 = fb.ReadUInt32();
      }
      Texture2D texture2D;
      if (num2 == 18)
      {
        if (!flag)
          throw new Exception("Can't read DXT1 data that is not flat packed");
        texture2D = new Texture2D(GraphicsDevice, width, height, 0, TextureUsage.None, SurfaceFormat.Dxt1);
        int num10 = height;
        for (int level = 0; level < length2; ++level)
        {
          int num11;
          byte[] data;
          if (num10 > 2)
          {
            num11 = fb.ReadInt32();
            data = fb.ReadBytes(num11);
            texture2D.SetData<byte>(level, new Rectangle?(), data, 0, num11, SetDataOptions.None);
          }
          else
          {
            fb.ReadBytes(num10 * num10 * 3);
            num11 = 8;
            data = new byte[8];
          }
          texture2D.SetData<byte>(level, new Rectangle?(), data, 0, num11, SetDataOptions.None);
          num10 /= 2;
          if (num10 < 1)
            num10 = 1;
        }
      }
      else
      {
        if (flag)
          throw new Exception("Can't read nonDXT1 data that is flat packed");
        texture2D = length2 != 1 ? new Texture2D(GraphicsDevice, width, height, 0, TextureUsage.None, SurfaceFormat.Color) : new Texture2D(GraphicsDevice, width, height, 1, TextureUsage.AutoGenerateMipMap, SurfaceFormat.Color);
        int num10 = height;
        int num11 = width;
        for (int level = 0; level < length2; ++level)
        {
          if (num10 < 1)
            num10 = 1;
          if (num10 < 1)
            num10 = 1;
          int num12 = -1;
          int num13 = -1;
          int num14 = -1;
          int num15 = -1;
          int num16 = -1;
          int num17 = 0;
          foreach (ACEFile.ACEChannel aceChannel in aceChannelArray)
          {
            switch (aceChannel.id)
            {
              case 2:
                num16 = num17;
                break;
              case 3:
                num12 = num17;
                break;
              case 4:
                num14 = num17;
                break;
              case 5:
                num13 = num17;
                break;
              case 6:
                num15 = num17;
                break;
            }
            switch (aceChannel.nbits)
            {
              case 1:
                num17 += 1 + (num11 - 1) / 8;
                break;
              case 8:
                num17 += num11;
                break;
            }
          }
          if (num15 != -1)
            num16 = -1;
          int count = num17;
          Color[] data = new Color[num11 * num10];
          int num18 = 0;
          for (int index1 = 0; index1 < num10; ++index1)
          {
            byte[] numArray2 = fb.ReadBytes(count);
            for (int index2 = 0; index2 < num11; ++index2)
            {
              byte r = num12 == -1 ? (byte) 0 : numArray2[num12 + index2];
              byte g = num14 == -1 ? (byte) 0 : numArray2[num14 + index2];
              byte b = num13 == -1 ? (byte) 0 : numArray2[num13 + index2];
              byte a = num15 == -1 ? byte.MaxValue : numArray2[num15 + index2];
              if (num16 != -1 && ((int) (byte) ((uint) numArray2[num16 + index2 / 8] << (int) (byte) (index2 % 8)) & 128) != 128)
                a = (byte) 0;
              data[num18++] = new Color(r, g, b, a);
            }
          }
          texture2D.SetData<Color>(level, new Rectangle?(), data, 0, num11 * num10, SetDataOptions.None);
          num10 /= 2;
          num11 /= 2;
        }
        if (length2 == 1)
          texture2D.GenerateMipMaps(TextureFilter.Linear);
      }
      return texture2D;
    }

    private class ACEChannel
    {
      public int nbits;
      public int palette;
      public int id;
      public int reserved;

      public ACEChannel(BinaryReader fb)
      {
        this.nbits = fb.ReadInt32();
        this.palette = fb.ReadInt32();
        this.id = fb.ReadInt32();
        this.reserved = fb.ReadInt32();
      }
    }
  }
}

*/