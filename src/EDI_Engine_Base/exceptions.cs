using System;

namespace EDI
{
	public class MissingAddressFieldException : ApplicationException
	{
        public MissingAddressFieldException() : base() {}
	    public MissingAddressFieldException(string err) : base(err) {}
        public MissingAddressFieldException(string err, Exception inner) :
            base(err, inner) {}
	}

    public class MissingContactFieldException : Exception
    {
        public MissingContactFieldException() : base() {}
        public MissingContactFieldException(string err) : base(err) {}
        public MissingContactFieldException(string err, Exception inner) :
            base(err, inner) {}
    }

    public class WrongNumberOfTransactionsException : Exception
    {
        public WrongNumberOfTransactionsException() : base() {}
        public WrongNumberOfTransactionsException(string err) : base(err) {}
        public WrongNumberOfTransactionsException(string err, Exception inner) :
            base(err, inner) {}
    }
}
