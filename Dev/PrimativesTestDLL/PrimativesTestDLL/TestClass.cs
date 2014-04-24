
namespace Dev2.PrimitiveTestDLL
{
    /// <summary>
    /// Test Primitive Return Types for DLLs
    /// </summary>
    public class TestClass
    {
        public string FetchStringValue(string name)
        {
            return "Hello, " + name;
        }

        public string FetchXmlString(string name)
        {
            return "<Message>Howdy, " + name + "</Message>";
        }

        public string FetchJsonString(string name)
        {
            return "{ \"message\" : \"Howzit, " + name + "\" }";
        }

        public char FetchCharValue()
        {
            return 'z';
        }

        public int FetchIntValue(int myValue)
        {
            return myValue + 1;
        }

        public double FetchDoubleValue(double myValue)
        {
            return myValue + 1.0;
        }

        public bool FetchBoolean(bool myBool)
        {
            return !myBool;
        }

        public object FetchObjectValue()
        {
            return "myObject";
        }

        #region primitive arrays

        public char[] FetchCharArrayValue(string msg)
        {
            return msg.ToCharArray();
        }

        public int[] FetchIntArrayValue(int value, int count)
        {
            if(count > 0)
            {
                int[] result = new int[count];
                for(int i = 0; i < count; i++)
                {
                    result[i] = value + i;
                }

                return result;
            }

            return null;
        }

        public string[] FetchStringArrayValue(string msg, int count)
        {
            if(count > 0)
            {
                string[] result = new string[count];
                for(int i = 0; i < count; i++)
                {
                    result[i] = msg;
                }

                return result;
            }

            return null;
        }

        #endregion
    }
}
