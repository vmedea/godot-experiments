using System;
using System.Text;
using System.IO;

  public static class ExtensionByte {
	public static void Init (this byte [] buffer, byte b) {
	  for (int i = 0; i < buffer.Length; i++)
		buffer [i] = b;
	}

	public static void WriteToFile (this byte [] buffer, string filename) {
	  using (Stream stream = new FileStream (filename, FileMode.Create, FileAccess.Write)) {
		stream.Write (buffer, 0, buffer.Length);
		stream.Close ();
	  }
	}

	// byte
	public static int ReadByte (this byte [] buffer, int offset) {
	  int value = buffer [offset];
	  return (value);
	}

	public static void WriteByte (this byte [] buffer, int offset, int value) {
	  buffer [offset] = (byte) ((value >> 0) & 0xFF);
	}

	// word
	public static int ReadWord (this byte [] buffer, int offset) {
	  int value = (buffer [offset + 0] << 8) |
				  (buffer [offset + 1] << 0);
	  return (value);
	}

	public static void WriteWord (this byte [] buffer, int offset, int value) {
	  buffer [offset + 0] = (byte) ((value >> 8) & 0xFF);
	  buffer [offset + 1] = (byte) ((value >> 0) & 0xFF);
	}

	// long
	public static long ReadLong (this byte [] buffer, int offset) {
	  long value = ((buffer [offset + 0] << 24) |
					(buffer [offset + 1] << 16) |
					(buffer [offset + 2] << 08) |
					(buffer [offset + 3] << 00)) & 0xFFFFFFFF;
	  return (value);
	}

	public static void WriteLong (this byte [] buffer, int offset, long value) {
	  buffer [offset + 0] = (byte) ((value >> 24) & 0xFF);
	  buffer [offset + 1] = (byte) ((value >> 16) & 0xFF);
	  buffer [offset + 2] = (byte) ((value >> 08) & 0xFF);
	  buffer [offset + 3] = (byte) ((value >> 00) & 0xFF);
	}

	public static string Dump (this byte [] buffer, int offset, int length, int baseAddress, int typeSize, int width, bool showAddress) {
	  StringBuilder sb = new StringBuilder ();
	  int firstAddress = offset;
	  if (length == -1) {
		length = buffer.Length - firstAddress;
	  }
	  int i = 0;
	  for (; ; ) {
		if ((offset - firstAddress) >= length) {
		  break;
		}
		if ((i == 0) && (offset != firstAddress)) {
		  sb.AppendLine ();
		}
		long value = -1;
		if ((offset + typeSize) <= buffer.Length) {
		  if (typeSize == 1) {
			value = buffer.ReadByte (offset);
		  }
		  else if (typeSize == 2) {
			value = buffer.ReadWord (offset);
		  }
		  else if (typeSize == 4) {
			value = buffer.ReadLong (offset);
		  }
		}

		if (showAddress) {
		  if (i == 0) {
			sb.Append (string.Format ("{0:X8}  ", baseAddress + offset - firstAddress));
		  }
		}

		if (typeSize == 1) {
		  sb.Append (string.Format ("{0:X2} ", value));
		}
		else if (typeSize == 2) {
		  sb.Append (string.Format ("{0:X4} ", value));
		}
		else if (typeSize == 4) {
		  sb.Append (string.Format ("{0:X8} ", value));
		}

		offset += typeSize;
		if (width != -1) {
		  i = (i + 1) % width;
		}
		else {
		  i = 1;
		}
	  }

	  return (sb.ToString ());
	}

  }
