// Camera configuration data.
public class CameraConfig
{
    public int resolutionWidth;
    public int resolutionHeight;
    public double pixelSize; // in millimeters (e.g., 2.4um = 0.0024 mm)
    public int frameRate;

    public CameraConfig(int resW, int resH, double pixelSize, int frameRate)
    {
        this.resolutionWidth = resW;
        this.resolutionHeight = resH;
        this.pixelSize = pixelSize;
        this.frameRate = frameRate;
    }
}

// Lens configuration data.
public class LensConfig
{
    public double focalLength; // in millimeters
    public double minWorkingDistance; // in millimeters, defines the near clip plane
    public double maxWorkingDistance; // in millimeters, defines the far clip plane

    public LensConfig(double focalLength, double minWorkingDistance, double maxWorkingDistance)
    {
        this.focalLength = focalLength;
        this.minWorkingDistance = minWorkingDistance;
        this.maxWorkingDistance = maxWorkingDistance;
    }
}
