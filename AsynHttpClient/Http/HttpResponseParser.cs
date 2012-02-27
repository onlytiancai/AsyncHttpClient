using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AsynHttpClient.Tcp;

namespace AsynHttpClient.Http
{
    internal class HttpResponseParser
    {
        public string Parse(BufferData data)//info:noexception
        {
            if (data == null || data.Length == 0) return null;
            int count = _decoder.GetChars(data.Buffer, data.Offset, data.Length, _charbuffers, _count);
            _count += count;
            if (ParseEnd())
                return GetResult();
            return null;
        }

        private bool ParseEnd()
        {
            if (_count >= MAX_CHARS)
                return true;
            if (HttpHeaderEnd())
                return true;
            return false;
        }

        private bool HttpHeaderEnd()
        {
            for (int i = 0; i <= MAX_CHARS - 4; i++)
            {
                if (_charbuffers[i] == '\r' && _charbuffers[i + 1] == '\n'
                    && _charbuffers[i + 2] == '\r' && _charbuffers[i + 3] == '\n')
                    return true;
            }
            return false;
        }

        private string GetResult()
        {
            int count = _count > MAX_CHARS ? MAX_CHARS : _count;
            return new string(_charbuffers, 0, count);
        }

        private Decoder _decoder = System.Text.ASCIIEncoding.ASCII.GetDecoder();
        private char[] _charbuffers = new char[MAX_CHARS];
        private int _count = 0;
        private static readonly int MAX_CHARS = 1024;
    }
}
