namespace Application.Exceptions;

public class ImageNotFoundException(string message) : Exception(message);

public class ImageOperationException(string message) : Exception(message);

public class ImageUploadException(string message) : Exception(message);

public class ImageDeletionException(string message) : Exception(message);

