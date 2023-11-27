using System;
using System.IO;
using System.Reflection;
using System.Text;


namespace YawVR {
    public static class Helpers {

        public static T[] SubArray<T>(this T[] data, int index, int length) {
            T[] result = new T[length];
            Array.Copy(data, index, result, 0, length);
            return result;
        }

        public static float decodeFloat(byte[] msg, int offset) {
            byte[] array = new byte[4];

            for (int i = 0; i < 3; i++) {
                array[i] = msg[offset + i];
            }

            return floatConversion(array);

        }
        public static void FromShort(ushort number, out byte byte1, out byte byte2) {
            byte2 = (byte)(number >> 8);
            byte1 = (byte)(number & 255);
        }


        // Generate a random string with a given size  
        public static string RandomString(int size, bool lowerCase) {
            StringBuilder builder = new StringBuilder();
            Random random = new Random();
            char ch;
            for (int i = 0; i < size; i++) {
                ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
                builder.Append(ch);
            }
            if (lowerCase)
                return builder.ToString().ToLower();
            return builder.ToString();
        }
        public static bool ContainsAny(string haystack, params string[] needles) {
            foreach (string needle in needles) {
                if (haystack.Contains(needle))
                    return true;
            }

            return false;
        }
        public static float ReadSingle(byte[] data, int offset, bool littleEndian) {
            if (BitConverter.IsLittleEndian != littleEndian) {   // other-endian; reverse this portion of the data (4 bytes)
                byte tmp = data[offset];
                data[offset] = data[offset + 3];
                data[offset + 3] = tmp;
                tmp = data[offset + 1];
                data[offset + 1] = data[offset + 2];
                data[offset + 2] = tmp;
            }
            return BitConverter.ToSingle(data, offset);
        }


        public static UInt32 ReadInt(byte[] data, int offset, bool littleEndian) {
            if (BitConverter.IsLittleEndian != littleEndian) {   // other-endian; reverse this portion of the data (4 bytes)
                byte tmp = data[offset];
                data[offset] = data[offset + 3];
                data[offset + 3] = tmp;
                tmp = data[offset + 1];
                data[offset + 1] = data[offset + 2];
                data[offset + 2] = tmp;
            }
            return BitConverter.ToUInt32(data, offset);
        }
        /// <summary>
        ///   <para>Clamps value between 0 and 1 and returns value.</para>
        /// </summary>
        /// <param name="value"></param>
        public static float Clamp01(float value) {
            if ((double)value < 0.0)
                return 0.0f;
            if ((double)value > 1.0)
                return 1f;
            return value;
        }

        public static float ReadSingle(float roll) {
            throw new NotImplementedException();
        }

        /// <summary>
        ///   <para>Linearly interpolates between a and b by t.</para>
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="t"></param>
        public static float Lerp(float a, float b, float t) {
            return a + (b - a) * Clamp01(t);
        }
        public static float floatConversion(byte[] bytes) {
            if (BitConverter.IsLittleEndian) {
                Array.Reverse(bytes); // Convert big endian to little endian
            }
            float myFloat = BitConverter.ToSingle(bytes, 0);
            return (float)Math.Round(myFloat, 3);
        }
        public static float ClampBetween(float v, float min, float max) {
            if (v < min) return min;
            if (v > max) return max;
            return v;
        }
        public static float Clamp(float v, float limit) {
            if (limit == -1) return v;
            if (v > limit) return limit;
            if (v < -limit) return -limit;
            return v;
        }

        public static float NormalizeAngle(float angle) {
            float newAngle = angle;
            while (newAngle <= -180) newAngle += 360;
            while (newAngle > 180) newAngle -= 360;
            return newAngle;
        }
       
        public static string StringToFilename(string s) {
            Array.ForEach(Path.GetInvalidFileNameChars(),
                c => s = s.Replace(c.ToString(), "_"));

            return s;
        }

    
        public static double RadianToDegree(double angle) {
            return angle * (180.0 / Math.PI);
        }

        #region byte operations
        public static float ByteArrayToFloat(byte[] floatBytes, int startIndex) {
            byte[] floatArray = new byte[4] { floatBytes[startIndex], floatBytes[startIndex + 1], floatBytes[startIndex + 2], floatBytes[startIndex + 3] };
            if (BitConverter.IsLittleEndian) {
                Array.Reverse(floatArray);
            }
            float floatNumber = BitConverter.ToSingle(floatArray, 0);
            return floatNumber;
        }
        public static byte[] AddByteToArray(byte[] bArray, byte newByte) {
            byte[] newArray = new byte[bArray.Length + 1];
            bArray.CopyTo(newArray, 1);
            newArray[0] = newByte;
            return newArray;
        }

        public static byte[] IntToByteArray(int intValue) {
            byte[] intBytes = BitConverter.GetBytes(intValue);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(intBytes);
            return intBytes;
        }

        public static byte[] FloatToByteArray(float floatValue) {
            byte[] floatBytes = BitConverter.GetBytes(floatValue);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(floatBytes);
            return floatBytes;
        }

        public static byte[] BoolToByteArray(bool boolValue) {
            return BitConverter.GetBytes(boolValue);
        }

        public static int ByteArrayToInt(byte[] intBytes, int startIndex) {
            byte[] intArray = new byte[4] { intBytes[startIndex], intBytes[startIndex + 1], intBytes[startIndex + 2], intBytes[startIndex + 3] };
            if (BitConverter.IsLittleEndian) {
                Array.Reverse(intArray);
            }
            int integer = BitConverter.ToInt32(intArray, 0);
            return integer;
        }



        public static bool ByteArrayToBool(byte[] boolBytes, int startIndex) {
            bool boolValue = BitConverter.ToBoolean(boolBytes, startIndex);
            return boolValue;
        }
        #endregion
    }
}
