using System.IO;

namespace Lib
{
    public class LZW
    {
        private Table[] _codeTable = new Table[0x4000];
        private byte[] _codeCur = new byte[256];
        private uint _bitsData, _bitsSize;
        private uint _codeSize, _codeLen, _cacheBits;
        private uint _tableSize, _tableMax;
        private bool _tableFull;

        protected uint GetCode(uint totalBits, BinaryReader input)
        {
            uint result, numBits;
            byte[] bitMasks = new byte[9] { 0x00, 0x01, 0x03, 0x07, 0x0f, 0x1f, 0x3f, 0x7f, 0xff };

            numBits = totalBits;
            result = 0;

            while (numBits > 0)
            {
                uint useBits;

                if (input.BaseStream.Position >= input.BaseStream.Length) { return 0xffffffff; }

                if (_bitsSize == 0)
                {
                    _bitsSize = 8;
                    _bitsData = input.ReadByte();
                }

                useBits = numBits;
                if (useBits > 8) { useBits = 8; }
                if (useBits > _bitsSize) { useBits = _bitsSize; }

                result |= (_bitsData & bitMasks[useBits]) << (int)(totalBits - numBits);

                numBits -= useBits;
                _bitsSize -= useBits;
                _bitsData >>= (int)useBits;
            }

            return result;
        }
        public byte[] Decompress(uint sz, BinaryReader input)
        {
            byte[] dest = new byte[sz];
            _bitsData = 0;
            _bitsSize = 0;

            Reset();

            uint idx;
            idx = 0;
            _cacheBits = 0;

            do
            {
                uint code;

                code = GetCode(_codeSize, input);
                if (code == 0xffffffff) { break; }

                _cacheBits += _codeSize;
                if (_cacheBits >= _codeSize * 8)
                {
                    _cacheBits -= _codeSize * 8;
                }

                if (code == 0x100)
                {
                    if (_cacheBits > 0)
                    {
                        GetCode(_codeSize * 8 - _cacheBits, input);
                        Reset();
                    }
                }
                else
                {
                    if (code >= _tableSize && !_tableFull)
                    {
                        _codeCur[_codeLen++] = _codeCur[0];

                        for (uint i = 0; i < _codeLen; i++)
                        {
                            dest[idx++] = _codeCur[i];
                        }
                    }
                    else
                    {
                        for (uint i = 0; i < _codeTable[code].len; i++)
                        {
                            dest[idx++] = _codeTable[code].str[i];
                        }

                        _codeCur[_codeLen++] = _codeTable[code].str[0];
                    }

                    if (_codeLen >= 2)
                    {
                        if (!_tableFull)
                        {
                            uint i;

                            if (_tableSize == _tableMax && _codeSize == 12)
                            {
                                _tableFull = true;
                                i = _tableSize;
                            }
                            else
                            {
                                i = _tableSize++;
                                _cacheBits = 0;
                            }

                            if (_tableSize == _tableMax && _codeSize < 12)
                            {
                                _codeSize++;
                                _tableMax <<= 1;
                            }

                            for (uint j = 0; j < _codeLen; j++)
                            {
                                _codeTable[i].str[j] = _codeCur[j];
                                _codeTable[i].len++;
                            }
                        }

                        for (uint i = 0; i < _codeTable[code].len; i++)
                        {
                            _codeCur[i] = _codeTable[code].str[i];
                        }

                        _codeLen = _codeTable[code].len;
                    }
                }
            } while (idx < sz);

            return dest;
        }
        public void Reset()
        {
            for (uint i = 0; i < _codeTable.Length; i++)
            {
                _codeTable[i] = new Table();
            }

            for (uint code = 0; code < 256; code++)
            {
                _codeTable[code].len = 1;
                _codeTable[code].str[0] = (byte)code;
            }

            _tableSize = 0x101;
            _tableMax = 0x200;
            _tableFull = false;

            _codeSize = 9;
            _codeLen = 0;

            _cacheBits = 0;
        }
    }

    public class Table
    {
        public byte[] str = new byte[256];
        public byte len;
    }
}
