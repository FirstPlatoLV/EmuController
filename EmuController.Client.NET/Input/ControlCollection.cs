using System;
using System.Collections;

namespace EmuController.Client.NET.Input
{
    public abstract class ControlCollection<T> where T : IComparable, IComparable<T>, IConvertible, IEquatable<T>, IFormattable
    {
        protected readonly BitArray arrayMap = new BitArray(8);

        internal byte ArrayMap
        {
            get
            {
                byte[] b = new byte[1];
                arrayMap.CopyTo(b, 0);
                return b[0];
            }
        }

        private int setControls = 0;

        protected T[] values;
        public ReadOnlySpan<T> AllValues => values;

        public ReadOnlySpan<T> GetUpdatedValues()
        {
            if (setControls == 0)
            {
                return null;
            }

            T[] results = new T[setControls];
            int index = 0;
            for (int i = 0; i < arrayMap.Count; i++)
            {
                if (arrayMap[i])
                {
                    results[index] = values[i];
                    index++;
                }
            }
            return results;

        }
        
        protected void SetValue(int index, T value)
        {
            if (!arrayMap.Get(index))
            {
                setControls++;
            }
            arrayMap.Set(index, true);
            values[index] = value;
        }
    }
}
