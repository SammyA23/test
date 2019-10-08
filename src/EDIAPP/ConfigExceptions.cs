using System;

namespace EDIAPP
{
  public class TooManyConfigsFoundException : Exception
  {
    public TooManyConfigsFoundException()
    {

    }

    public TooManyConfigsFoundException(string message) : base(message)
    {

    }

    public TooManyConfigsFoundException(string message, Exception inner)
        : base(message, inner)
    {

    }
  }

  public class NoConfigsFoundException : Exception
  {
    public NoConfigsFoundException()
    {

    }

    public NoConfigsFoundException(string message) : base(message)
    {

    }

    public NoConfigsFoundException(string message, Exception inner)
        : base(message, inner)
    {

    }
  }
}
