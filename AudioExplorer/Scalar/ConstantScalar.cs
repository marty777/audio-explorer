using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioExplorer.Scalar
{
    class ConstantScalar : Scalar
    {
        private float _val;

        public float Value {
            get { return _val; }
            set
            {
                if (value < -1 || value > 1)
                    throw new ArgumentOutOfRangeException("value must be between -1 and 1 inclusive");
                _val = value;
            }
        }

        public ConstantScalar(float value)
        {
            _val = value;
        }

        public override int Read(float[] buffer, int offset, int count)
        {
            for (int i = offset; i < count; i++)
            {
                buffer[i] = _val;
            }

            return count;
        }
}
