/// <summary>
/// Indicates that the program could not be parsed because it was empty.
/// </summary>
public class EmptyProgramException() : Exception("Grammar does not support an empty program.") { }
