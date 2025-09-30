using System;

namespace LibrarySystem.BusinessLogic.Domain.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class IgnoreOnInsertAttribute : Attribute
{
}
