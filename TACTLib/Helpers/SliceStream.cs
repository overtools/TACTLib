using System;
using System.IO;

namespace TACTLib.Helpers {
    public class SliceStream : Stream {
        private bool m_leaveOpen;
        private readonly Stream m_innerStream;
        private readonly long m_origin;
        private readonly long m_length;
        
        public SliceStream(Stream stream, int length, bool leaveOpen=false) : 
            this(stream, stream.Position, length, leaveOpen)
        {
        }

        public SliceStream(Stream stream, long origin, long mLength, bool leaveOpen=false) {
            m_innerStream = stream;
            m_origin = origin;
            m_length = mLength;
            m_leaveOpen = leaveOpen;
        }

        public override long Seek(long offset, SeekOrigin origin) {
            switch (origin) {
                case SeekOrigin.Begin:
                    return m_innerStream.Seek(offset + m_origin, SeekOrigin.Begin);
                case SeekOrigin.Current:
                    return m_innerStream.Seek(offset, SeekOrigin.Current);
                //case SeekOrigin.End:
                //return _baseStream.Seek(_length - offset + _origin, SeekOrigin.End);
                default:
                    throw new NotImplementedException();
            }
        }

        public override void SetLength(long value) => throw new NotSupportedException();
        
        public override void Flush() {
            m_innerStream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count) {
            var count2 = Length - Position;
            if (count2 > count)
                count2 = count;
            if (count2 <= 0)
                return 0;
            
            return m_innerStream.Read(buffer, offset, (int)count2);
        }

        public override void Write(byte[] buffer, int offset, int count) {
            m_innerStream.Write(buffer, offset, count);
        }

        public override bool CanRead => m_innerStream.CanRead;

        public override bool CanSeek => m_innerStream.CanSeek;

        public override bool CanWrite => m_innerStream.CanWrite;

        public override long Length => m_length;

        public override long Position {
            get => m_innerStream.Position - m_origin;
            set => m_innerStream.Position = value + m_origin;
        }

        protected override void Dispose(bool disposing)
        {
            if (!m_leaveOpen) {
                m_innerStream?.Dispose();
            }
        }
    }
}
