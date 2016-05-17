using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colsp.Logic
{
    public enum OperationResultCode
    {
        [Description("Fail")]
        Fail,

        [Description("Ok")]
        Ok,
    }

    public class OperationResult
    {
        public OperationResult()
        {
            Tracing = new List<string>();
        }
        public bool IsSuccess { get; set; }
        public OperationResultCode ResultCode { get; set; }
        public string ResultText { get; set; }
        public Exception Ex { set; get; }

        public List<string> Tracing { set; get; }
    }

    public class OperationResultType<T>
    {
        public OperationResultType()
        {
            Tracing = new List<string>();
        }

        private OperationResultCode _ResultCode;
        public OperationResultCode ResultCode
        {
            get
            {
                if (_ResultCode == null)
                {
                    _ResultCode = OperationResultCode.Fail;
                }
                return _ResultCode;
            }
            set
            {
                _ResultCode = value;
            }
        }

        public bool IsSuccess { get; set; }
        public string ResultText { get; set; }
        public Exception Ex { set; get; }

        public T Result { set; get; }
        public List<string> Tracing { set; get; }

        private IList<T> _Items = new List<T>();
        public IList<T> Items
        {
            set
            {
                _Items = value;
            }
            get
            {
                return _Items;
            }
        }
    }
}
