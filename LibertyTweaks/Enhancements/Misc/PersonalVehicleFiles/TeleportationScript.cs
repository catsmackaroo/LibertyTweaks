using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System;
using LibertyTweaks;

public class TeleportationScript
{
    private List<TeleportLocation> teleportLocations;

    public TeleportationScript()
    {
        teleportLocations = new List<TeleportLocation>();
        LoadTeleportLocationsFromFile("IVSDKDotNet/scripts/LibertyTweaks/PersonalVehicleFiles/TeleportLocations.txt");
    }

    private void LoadTeleportLocationsFromFile(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                LibertyTweaks.Main.Log($"File not found: {filePath}");
                return;
            }

            using (StreamReader reader = new StreamReader(filePath))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    string[] parts = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 4)
                    {
                        if (float.TryParse(parts[0], out float x) &&
                            float.TryParse(parts[1], out float y) &&
                            float.TryParse(parts[2], out float z) &&
                            float.TryParse(parts[3], out float heading))
                        {
                            teleportLocations.Add(new TeleportLocation(x, y, z, heading));
                        }
                        else
                        {
                            LibertyTweaks.Main.Log($"Error parsing line: {line}. Skipping...");
                        }
                    }
                    else
                    {
                        LibertyTweaks.Main.Log($"Invalid line format: {line}. Skipping...");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            LibertyTweaks.Main.Log($"Error loading teleport locations from file: {ex.Message}");
        }
    }

    public TeleportLocation GetNearestTeleportLocation(Vector3 playerPosition)
    {
        TeleportLocation nearestLocation = null;
        float minDistance = float.MaxValue;

        foreach (var location in teleportLocations)
        {
            float distance = CalculateDistance(playerPosition.X, playerPosition.Y, playerPosition.Z, location.X, location.Y, location.Z);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearestLocation = location;
            }
        }

        return nearestLocation;
    }

    private float CalculateDistance(float x1, float y1, float z1, float x2, float y2, float z2)
    {
        return (float)Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2) + Math.Pow(z2 - z1, 2));
    }
}

public class TeleportLocation
{
    public float X { get; }
    public float Y { get; }
    public float Z { get; }
    public float Heading { get; }

    public TeleportLocation(float x, float y, float z, float heading)
    {
        X = x;
        Y = y;
        Z = z;
        Heading = heading;
    }

    public override string ToString()
    {
        return $"X={X}, Y={Y}, Z={Z}, Heading={Heading}";
    }

    public Vector3 ToVector3()
    {
        return new Vector3(X, Y, Z);
    }
}
