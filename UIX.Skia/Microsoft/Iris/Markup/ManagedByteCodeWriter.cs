using Microsoft.Iris.OS;
using System;
using System.Collections;
using System.IO;
using System.Runtime.InteropServices;

namespace Microsoft.Iris.Markup
{
    internal class ManagedByteCodeWriter
    {
        private const int BLOCK_SIZE = 4096;
        private byte[] _scratch = new byte[8];
        private BinaryWriter _writer;
        private MemoryStream _stream;
        private uint _cbFreeInBlock;
        private uint _totalSize;
        private byte[] _currentBlock;
        private ArrayList _blockList = new ArrayList();

        internal ManagedByteCodeWriter()
        {
            _stream = new MemoryStream();
            _writer = new BinaryWriter(_stream);
        }

        public uint DataSize => _totalSize;

        public void WriteByte(byte value) => _writer.Write(value);

        public void WriteByte(OpCode value) => _writer.Write((byte)value);

        public void WriteBool(bool value) => _writer.Write(value ? 1 : 0);

        public void WriteChar(char value) => _writer.Write(value);

        public void WriteUInt16(ushort value) => _writer.Write(value);

        public void WriteUInt16(int rawValue) => WriteUInt16((ushort)rawValue);

        public void WriteUInt16(uint rawValue) => WriteUInt16((ushort)rawValue);

        public void WriteInt32(int value) => _writer.Write(value);

        public void WriteUInt32(uint value) => _writer.Write(value);

        public void WriteInt64(long value) => _writer.Write(value);

        public void WriteUInt64(ulong value) => _writer.Write(value);

        public void WriteSingle(float value) => _writer.Write(value);

        public unsafe void WriteDouble(double value) => _writer.Write(value);

        public void WriteString(string value)
        {
            if (value == null)
            {
                WriteUInt16(ushort.MaxValue);
            }
            else
            {
                if (value.Length >= short.MaxValue)
                    throw new ArgumentException("String too long");
                bool flag = false;
                foreach (char ch in value)
                {
                    if (ch > 'ÿ')
                    {
                        flag = true;
                        break;
                    }
                }
                uint length = (uint)value.Length;
                if (!flag)
                    length |= 32768U;
                WriteUInt16((ushort)length);
                foreach (char ch in value)
                {
                    if (flag)
                        WriteChar(ch);
                    else
                        WriteByte((byte)ch);
                }
            }
        }

        public void Write(ManagedByteCodeReader value) => Write(value, 0U);

        public unsafe void Write(ManagedByteCodeReader value, uint offset)
        {
            IntPtr intPtr = value.ToIntPtr(out long size);
            if (offset > size)
                throw new ArgumentOutOfRangeException(nameof(offset));
            Write(new IntPtr(intPtr.ToInt64() + offset), (uint)(size - offset));
        }

        public unsafe void Write(byte[] buffer, uint count)
        {
            fixed (byte* pbData = buffer)
                Write(pbData, count);
        }

        public unsafe void Write(IntPtr buffer, uint count) => Write((byte*)buffer.ToPointer(), count);

        private unsafe void Write(byte* pbData, uint cbData)
        {
            while (cbData > 0U)
            {
                if (_cbFreeInBlock == 0U)
                {
                    _currentBlock = new byte[4096];
                    _blockList.Add(_currentBlock);
                    _cbFreeInBlock = 4096U;
                }
                uint num1 = cbData <= _cbFreeInBlock ? cbData : _cbFreeInBlock;
                uint num2 = 4096U - _cbFreeInBlock;
                Marshal.Copy(new IntPtr(pbData), _currentBlock, (int)num2, (int)num1);
                pbData += (int)num1;
                cbData -= num1;
                _cbFreeInBlock -= num1;
                _totalSize += num1;
            }
        }

        public void Overwrite(uint offset, uint value)
        {
            if (offset + 4U > _totalSize)
                throw new ArgumentException("Invalid offset");
            OverwriteByte(offset, (byte)value);
            OverwriteByte(offset + 1U, (byte)(value >> 8));
            OverwriteByte(offset + 2U, (byte)(value >> 16));
            OverwriteByte(offset + 3U, (byte)(value >> 24));
        }

        private void OverwriteByte(uint offset, byte value) => ((byte[])_blockList[(int)(offset / 4096U)])[(offset % 4096U)] = value;

        public unsafe ManagedByteCodeReader CreateReader()
        {
            _writer.Flush();
            return new ManagedByteCodeReader(_stream.ToArray(), true);
        }
    }
}
