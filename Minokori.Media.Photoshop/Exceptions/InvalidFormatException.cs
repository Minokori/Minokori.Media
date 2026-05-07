namespace Minokori.Media.Photoshop.Exceptions;

public class InvalidFormatException(string? message = null) : Exception(message ?? "Invalid PSD file")
    {
    }

