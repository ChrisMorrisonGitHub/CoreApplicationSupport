using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniversalBinary.CoreApplicationSupport
{
    public sealed class BinaryToTextEncoder : IDisposable
    {
        private MemoryStream m_Stream;
        private Range m_range;

        /// <summary>
        /// Represents a range (start offset and length) of data that will be read from the underlying data source during the next encoding operation.
        /// </summary>
        public struct Range : IEquatable<Range>
        {
            internal Range(Int64 start, Int64 length)
            {
                this.Start = start;
                this.Length = length;
            }
            /// <summary>
            /// The location in the underlying data source to start reading data for the next encoding operation.
            /// </summary>
            public Int64 Start;
            /// <summary>
            /// The number of bytes in the underlying data source that should be used for the next encode operation.
            /// </summary>
            public Int64 Length;

            bool IEquatable<Range>.Equals(Range other)
            {
                return ((this.Length == other.Length) && (this.Start == other.Start));
            }
        }

        /// <summary>
        /// Creates a new instance of the BinaryToTextEncoder with the specified byte array as the underlying data source.
        /// </summary>
        /// <param name="data">A byte-array representing the data source for this instance.</param>
        public BinaryToTextEncoder(byte[] data)
        {
            if (data == null) throw new ArgumentNullException("data");
            if (data.LongLength == 0) throw new ArgumentOutOfRangeException("data", "data cannot be zero length.");
            m_range = new Range(0, data.LongLength); 
            m_Stream = new MemoryStream(data, false);
        }

        /// <summary>
        /// Creates a new instance of the BinaryToTextEncoder with the specified Stream object as the underlying data source.
        /// </summary>
        /// <param name="data">A Stream object representing the data source for this instance.</param>
        public BinaryToTextEncoder(System.IO.Stream data)
        {
            if (data == null) throw new ArgumentNullException("data");
            if (data.Length == 0) throw new ArgumentOutOfRangeException("data", "data cannot be zero length.");
            if (data.CanRead == false) throw new ArgumentException("The provided stream must be readable.", "data");
            if (data.GetType() == typeof(MemoryStream))
            {
                m_Stream = (MemoryStream)data;
            }
            else
            {
                m_Stream = new MemoryStream();
                data.Position = 0;
                data.CopyTo(m_Stream);
            }
            m_range = new Range(0, data.Length);
        }

        /// <summary>
        /// Gets or sets the Stream object the will serve as the underlying data source in subsequent operations.
        /// </summary>
        public System.IO.MemoryStream DataStream
        {
            get
            {
                return m_Stream;
            }
            set
            {
                if (m_Stream != null) m_Stream.Close();
                if (value == null) throw new ArgumentNullException("value");
                if (value.Length == 0) throw new ArgumentException("An empty (zero length) stream was passed.", "data");
                if (value.CanRead == false) throw new ArgumentException("The provided stream must be readable.", "data");
                if (value.GetType() == typeof(MemoryStream))
                {
                    m_Stream = (MemoryStream)value;
                }
                else
                {
                    m_Stream = new MemoryStream();
                    value.Position = 0;
                    value.CopyTo(m_Stream);
                }
                m_range = new Range(0, value.Length);
            }
        }

        /// <summary>
        /// Gets the length of the underlying data contained in this instance in bytes.
        /// </summary>
        public long DataLength
        {
            get
            {
                return m_Stream.Length;
            }
        }

        /// <summary>
        /// Sets the underlying data of this instance to the contents of the given byte array.
        /// </summary>
        /// <param name="data">The byte array containing the data to use as the underlying data.</param>
        public void SetDataFromArray(byte[] data)
        {
            if (data == null) throw new ArgumentNullException("value");
            if (data.Length == 0) throw new ArgumentException("An empty (zero length) buffer was passed.", "data");
            if (m_Stream != null) m_Stream.Close();
            m_Stream = new MemoryStream(data, false);
        }

        /// <summary>
        /// Get the underlying data of this instance as a byte array.
        /// </summary>
        /// <returns></returns>
        public byte[] ToArray()
        {
            return m_Stream.ToArray();
        }

        /// <summary>
        /// Gets or sets the range (start offset and length) of data that will be read from the underlying data source during the next encoding operation.
        /// </summary>
        public Range DataRange
        {
            get
            {
                return m_range;
            }
            set
            {
                m_range = value;
            }
        }

        /// <summary>
        /// Encodes the data contained in this instance to a Base2 (Binary) encoded string.
        /// </summary>
        /// <param name="addSpaces">A value to indicate whether the returned string should be spaced in groups of eight digits.</param>
        /// <returns>The encoded string.</returns>
        public string EncodeBase2String(bool addSpaces)
        {
            StringBuilder sb = new StringBuilder();
            string s;
            int b = 0;

            if ((m_range.Length + m_range.Start) > m_Stream.Length) throw new InvalidOperationException("The current range it outside of the bounds of the underlying stream.");

            m_Stream.Position = m_range.Start;
            for (long d = 0; d < m_range.Length; d++)
            {
                b = m_Stream.ReadByte();
                if (b == -1) throw new IOException("An unknown error occurred while reading from the underlying stream.");
                s = UnrollAndAppendByte((byte)b);
                sb.Append(s);
                if (addSpaces == true) sb.Append(" ");
            }

            return sb.ToString().Trim();
        }

        public void EncodeBase2String(string inString)
        {
            if (inString == null) throw new ArgumentNullException("inString");
            if (String.IsNullOrWhiteSpace(inString) == true) throw new ArgumentException("An empty string was passed.");
            inString = inString.Replace(" ", "");
            if ((inString.Length % 8) != 0) throw new ArgumentException("The given string was not a valid Base2 encoded string.");
            int bytes = inString.Length / 8;
            MemoryStream ms = new MemoryStream();
            byte b = 0;
            string substr;
            for (int it = 0; it < bytes; bytes++)
            {
                substr = inString.Substring((it * 8), 8);
                
                if (substr[7] == '1') b &= 0x01;
                if (substr[6] == '1') b &= 0x02;
                if (substr[5] == '1') b &= 0x04;
                if (substr[4] == '1') b &= 0x08;
                if (substr[3] == '1') b &= 0x10;
                if (substr[2] == '1') b &= 0x20;
                if (substr[1] == '1') b &= 0x40;
                if (substr[0] == '1') b &= 0x80;
                ms.WriteByte(b);
            }
            m_Stream.Close();
            m_range.Start = 0;
            m_range.Length = 0;
            m_Stream = ms;
        }

        private static string UnrollAndAppendByte(byte b)
        {
            StringBuilder ts = new StringBuilder(8);

            if ((b & 0x80) == 0x80)
                ts.Append("1");
            else
                ts.Append("0");

            if ((b & 0x40) == 0x40)
                ts.Append("1");
            else
                ts.Append("0");

            if ((b & 0x20) == 0x20)
                ts.Append("1");
            else
                ts.Append("0");

            if ((b & 0x10) == 0x10)
                ts.Append("1");
            else
                ts.Append("0");

            if ((b & 0x08) == 0x08)
                ts.Append("1");
            else
                ts.Append("0");

            if ((b & 0x04) == 0x04)
                ts.Append("1");
            else
                ts.Append("0");

            if ((b & 0x02) == 0x02)
                ts.Append("1");
            else
                ts.Append("0");

            if ((b & 0x01) == 0x01)
                ts.Append("1");
            else
                ts.Append("0");

            return ts.ToString();
        }

        /// <summary>
        /// Releases all the resources used by the BinaryToTextEncoder class.
        /// </summary>
        public void Dispose()
        {
            if (m_Stream != null) m_Stream.Dispose();
        }
    }
}
