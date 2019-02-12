// Copyright © 2019 Shawn Baker using the MIT License.
namespace RPiCameraViewer
{
	public class SpsReader
	{
		// instance variables
		private byte[] nal;
		private int length;
		private int currentBit;

		public SpsReader(byte[] nal, int length)
		{
			this.nal = nal;
			this.length = length;
			currentBit = 0;
		}

		public void SkipBits(int n)
		{
			currentBit += n;
		}

		public int ReadBit()
		{
			int index = currentBit / 8;
			int offset = currentBit % 8 + 1;

			currentBit++;
			return (index < length) ? ((nal[index] >> (8 - offset)) & 0x01) : 0;
		}

		public int ReadBits(int n)
		{
			int bits = 0;
			for (int i = 0; i < n; i++)
			{
				int bit = ReadBit();
				bits = (bits << 1) + bit;
			}
			return bits;
		}

		private int ReadCode(bool signed)
		{
			int zeros = 0;
			while (ReadBit() == 0)
			{
				zeros++;
			}

			int code = (1 << zeros) - 1 + ReadBits(zeros);
			if (signed)
			{
				code = (code + 1) / 2 * (code % 2 == 0 ? -1 : 1);
			}

			return code;
		}

		public int ReadExpGolombCode()
		{
			return ReadCode(false);
		}

		public int ReadSignedExpGolombCode()
		{
			return ReadCode(true);
		}

		public bool IsEnd()
		{
			return (currentBit / 8) >= length;
		}
	}
}
