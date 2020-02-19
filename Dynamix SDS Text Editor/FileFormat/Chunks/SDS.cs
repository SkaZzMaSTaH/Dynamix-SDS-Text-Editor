using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FileFormat.Chunks
{
    public class SDS
    {
        #region Private vars
        private char[] _id = new char[4];

        private byte _typeCompression;

        private byte[] _mark = new byte[4];
        private char[] _vers = new char[7];
        private ushort _index;

        private byte[] _data = new byte[] { };

        private List<Resource.SDS.Code> _segmentsCode = new List<Resource.SDS.Code>();
        private List<Resource.SDS.Message> _messages = new List<Resource.SDS.Message>();
        private List<ushort[]> _sizes = new List<ushort[]>();
        #endregion
        #region Public Properties
        public char[] ID { get { return _id; } }
        
        public byte TypeCompression { get { return _typeCompression; } }

        public byte[] Mark { get { return _mark; } }
        public char[] Version { get { return _vers; } }
        public ushort Index { get { return _index; } }

        public byte[] Data { get { return _data; } }

        public List<Resource.SDS.Code> SegmentsCode { get { return _segmentsCode; } }
        public List<Resource.SDS.Message> Messages { get { return _messages; } }
        public List<ushort[]> Sizes { get { return _sizes; } }

        public uint SizeChunk
        {
            get
            {
                int size = 18;

                foreach (Resource.SDS.Code code in _segmentsCode)
                {
                    size += code.Content.Length;
                }
                foreach (Resource.SDS.Message message in _messages)
                {
                    size += message.ToByte().Length;
                }

                return (uint)size;
            }
        }
        public uint SizeDecompress
        {
            get
            {
                return SizeChunk - 5;
            }
        }
        #endregion
        public SDS(char[] id, byte[] data)
        {
            _id = id;
            _data = data;

            ProcessDecode();
            ProcessHeader();
            ProcessPseudoDissembly();
            ProcessSizes();
        }
        #region Private Methods
        private void ProcessDecode()
        {
            BinaryReader bin = new BinaryReader(new MemoryStream(_data));

            uint sizeDecompress;

            using (bin)
            {
                _typeCompression = bin.ReadByte();
                sizeDecompress = bin.ReadUInt32();
                _data = bin.ReadBytes(_data.Length - 5);
            }

            if (_typeCompression == 0x02)
            {
                _data = Lib.Unpack.DecompressLZW(sizeDecompress, _data);
                _typeCompression = 0x00;
            }
        }
        private void ProcessHeader()
        {
            BinaryReader bin = new BinaryReader(new MemoryStream(_data));

            using (bin)
            {
                _mark = bin.ReadBytes(4);
                _vers = bin.ReadChars(7);
                _index = bin.ReadUInt16();
                _data = bin.ReadBytes(_data.Length - 13);
            }
        }
        private void ProcessPseudoDissembly()
        {
            const byte MSG_BYTE = 0x04;

            BinaryReader bin = new BinaryReader(new MemoryStream(_data));

            ushort opcodeFirst, opcodeSecond, opcodeThird;
            byte codeByte;
            List<byte> code = new List<byte>();

            ushort len;
            string content;

            ushort[] startOpcodes = new ushort[3];
            ushort[] unkValues = new ushort[3];

            _messages = new List<Resource.SDS.Message>();       // Reset
            _segmentsCode = new List<Resource.SDS.Code>();

            using (bin)
            {
                while (bin.BaseStream.Position < _data.Length)
                {
                    codeByte = bin.ReadByte();

                    if (codeByte == MSG_BYTE)
                    {
                        bin.BaseStream.Position -= 1;

                        opcodeFirst = bin.ReadUInt16();
                        opcodeSecond = bin.ReadUInt16();
                        opcodeThird = bin.ReadUInt16();

                        if ((opcodeFirst == Resource.SDS.Message.OpcodesFirst[0] &&         // 04 00 03 00 00 00
                            opcodeSecond == Resource.SDS.Message.OpcodesSecond[0] &&
                            opcodeThird == Resource.SDS.Message.OpcodeThird) ||
                            (opcodeFirst == Resource.SDS.Message.OpcodesFirst[1] &&         // 01 00 03 00 00 00
                            opcodeSecond == Resource.SDS.Message.OpcodesSecond[0] &&
                            opcodeThird == Resource.SDS.Message.OpcodeThird) ||
                            (opcodeFirst == Resource.SDS.Message.OpcodesFirst[0] &&         // 04 00 02 00 00 00
                            opcodeSecond == Resource.SDS.Message.OpcodesSecond[1] &&
                            opcodeThird == Resource.SDS.Message.OpcodeThird) ||
                            (opcodeFirst == Resource.SDS.Message.OpcodesFirst[0] &&         // 04 00 0b 00 00 00
                            opcodeSecond == Resource.SDS.Message.OpcodesSecond[2] &&
                            opcodeThird == Resource.SDS.Message.OpcodeThird))
                        {
                            _segmentsCode.Add(new Resource.SDS.Code(code.ToArray()));

                            code = new List<byte>();

                            startOpcodes[0] = opcodeFirst;
                            startOpcodes[1] = opcodeSecond;
                            startOpcodes[2] = opcodeThird;

                            unkValues[0] = bin.ReadUInt16();
                            unkValues[1] = bin.ReadUInt16();
                            unkValues[2] = bin.ReadUInt16();
                            len = bin.ReadUInt16();
                            content = Encoding.GetEncoding("437").GetString(bin.ReadBytes(len));

                            _messages.Add(new Resource.SDS.Message(startOpcodes, unkValues, content));
                        }
                        else
                        {
                            bin.BaseStream.Position -= 5;

                            code.Add(codeByte);
                        }
                    }
                    else
                    {
                        code.Add(codeByte);
                    }
                }

                _segmentsCode.Add(new Resource.SDS.Code(code.ToArray()));
            }

            _data = new byte[] { };
        }
        private void ProcessSizes()
        {
            Resource.SDS.Code currentCode = null;
            _sizes = new List<ushort[]>();

            int posWidth, posHeight;

            ushort[] currentSize;

            for (int i = 0; i < _messages.Count; i++)
            {
                currentCode = _segmentsCode[i];

                currentSize = new ushort[2];

                posWidth = currentCode.Content.Length - 12;
                posHeight = currentCode.Content.Length - 10;

                currentSize[0] = BitConverter.ToUInt16(new byte[2] { currentCode.Content[posWidth], currentCode.Content[posWidth + 1] }, 0);
                currentSize[1] = BitConverter.ToUInt16(new byte[2] { currentCode.Content[posHeight], currentCode.Content[posHeight + 1] }, 0);

                _sizes.Add(currentSize);
            }
        }
        #endregion
        #region Public Methods
        public void ReplaceMessage(int index, string content)
        {
            Resource.SDS.Message MessageSelect = _messages[index];

            MessageSelect.SetContentClean(content);
        }
        public void ReplaceSize(int index, ushort[] size)
        {
            byte[] width = BitConverter.GetBytes(size[0]);
            byte[] height = BitConverter.GetBytes(size[1]);

            Resource.SDS.Code SegmentCode = SegmentsCode[index];

            int posWidth = SegmentCode.Content.Length - 12;
            int posHeight = SegmentCode.Content.Length - 10;

            SegmentCode.SetCodeBytes(width, posWidth);
            SegmentCode.SetCodeBytes(height, posHeight);

            ProcessSizes();
        }

        public byte[] ToByte()
        {
            List<byte> data = new List<byte>();

            data.AddRange(Encoding.Default.GetBytes(_id));
            data.AddRange(BitConverter.GetBytes(SizeChunk));
            data.Add(_typeCompression);
            data.AddRange(BitConverter.GetBytes(SizeDecompress));
            data.AddRange(_mark);
            data.AddRange(Encoding.Default.GetBytes(_vers));
            data.AddRange(BitConverter.GetBytes(_index));

            for (int i = 0; i < _segmentsCode.Count; i++)
            {
                data.AddRange(_segmentsCode[i].Content);

                if (i < _messages.Count)
                {
                    data.AddRange(_messages[i].ToByte());
                }
            }
            
            return data.ToArray();
        }
        #endregion
    }
}
