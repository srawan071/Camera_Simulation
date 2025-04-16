using System.Collections.Generic;


// A simple configuration manager to map enum values to their configuration parameters.
public static class DeviceConfigManager
{
    // Camera configurations keyed by camera model.
    private static Dictionary<CameraModel, CameraConfig> cameraConfigs = new Dictionary<CameraModel, CameraConfig>()
    {
        { CameraModel.acA5472_5gm, new CameraConfig(5472, 3648, 0.0024, 5) },
        { CameraModel.acA5472_17um, new CameraConfig(5472, 3648, 0.0024, 17) }
    };

    // Lens configurations keyed by lens model.
    private static Dictionary<LensModel, LensConfig> lensConfigs = new Dictionary<LensModel, LensConfig>()
    {
        { LensModel.C11_0824_12M, new LensConfig(8, 100, 1750) },
        { LensModel.C11_1224_12M, new LensConfig(12, 100, 2500) },
        { LensModel.C11_1620_12M, new LensConfig(16, 100, 3500) },
        { LensModel.c12_1224_25M, new LensConfig(12, 100, 2500) },
        { LensModel.C12_1624_25M, new LensConfig(16, 100, 3500) },
        { LensModel.C12_2524_25M, new LensConfig(25, 150, 3500) },
        { LensModel.C12_3524_25M, new LensConfig(35, 150, 3500) },
        { LensModel.C12_5024_25M, new LensConfig(50, 200, 3500) }
    };

    public static CameraConfig GetCameraConfig(CameraModel model)
    {
        return cameraConfigs[model];
    }

    public static LensConfig GetLensConfig(LensModel model)
    {
        return lensConfigs[model];
    }
}