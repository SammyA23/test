﻿using System;

namespace EDIAPP
{
  public class DatabaseNotFoundException : Exception
  {
    public DatabaseNotFoundException()
    {

    }

    public DatabaseNotFoundException(string message) : base(message)
    {

    }

    public DatabaseNotFoundException(string message, Exception inner)
        : base(message, inner)
    {

    }
  }
}
