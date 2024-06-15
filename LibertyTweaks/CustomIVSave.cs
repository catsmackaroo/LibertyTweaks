using IVSDKDotNet;
using static IVSDKDotNet.Native.Natives;
using System.Drawing;
using System.Numerics;
using System.Windows.Forms;

/// <summary>
/// This class can be used to store data alongside the current save game that is loaded.
/// Still has some flavs tho.
/// </summary>
internal class CustomIVSave
{

    #region Variables and Enums
    // Variables
    private const string Version = "100";
    private SettingsFile Settings;
    private bool tempBool;

    // Enums
    private enum IVSaveGameSlot
    {
        SGTA400,
        SGTA401,
        SGTA402,
        SGTA403,
        SGTA404,
        SGTA405,
        SGTA406,
        SGTA407,
        SGTA408,
        SGTA409,
        SGTA410,
        SGTA411,
        SGTA412,
        SGTA413,
        SGTA414
    }
    #endregion

    #region Constructor
    private CustomIVSave(SettingsFile file)
    {
        Settings = file;
    }
    #endregion

    /// <summary>
    /// This creates a new custom save file, you can write data to, which will be stored for the currently loaded save slot.
    /// Or, it will load an already existing custom save file, named the same as your script but with the ".save" extension.
    /// </summary>
    /// <param name="targetScript">The target <see cref="Script"/> this custom save file is for.</param>
    /// <returns>A new instance of the <see cref="CustomIVSave"/> class, with which you can store data for the currently loaded save slot. Or <see langword="null"/> if the custom save file could neither be loaded or created.</returns>
    public static CustomIVSave CreateOrLoadSaveGameData(Script targetScript)
    {
        // Get file name for savegame settings file
        string scriptName = targetScript.GetName();
        string fileName;

        if (string.IsNullOrWhiteSpace(targetScript.ScriptResourceFolder))
            fileName = string.Format("{0}\\IVSDKDotNet\\scripts\\{1}.save", IVGame.GameStartupPath, scriptName);
        else
            fileName = string.Format("{0}\\{1}.save", targetScript.ScriptResourceFolder, scriptName);

        // Create or load settings file
        SettingsFile settings = null;

        if (System.IO.File.Exists(fileName))
        // Load
        {
            settings = new SettingsFile(fileName);

            if (settings.Load())
                return new CustomIVSave(settings);
        }
        else
        // Create and save
        {
            System.IO.File.WriteAllText(fileName, "");

            settings = new SettingsFile(fileName);

            settings.AddSection("CustomIVSave");
            settings.AddKeyToSection("CustomIVSave", "Version");
            settings.SetValue("CustomIVSave", "Version", Version);

            // Create save slot sections
            for (int i = 0; i < 15; i++)
                settings.AddSection(((IVSaveGameSlot)i).ToString());

            if (settings.Save())
                return new CustomIVSave(settings);
        }

        return null;
    }

    /// <summary>
    /// Checks if the game is currently saving.
    /// This can be used to write data to the custom save file (Like the current position of the player) and to <see cref="Save"/> this custom save file.
    /// </summary>
    /// <returns><see langword="true"/> if the game is saving and you can write custom data. Otherwise, <see langword="false"/>.</returns>
    public bool IsGameSaving()
    {
        if (DID_SAVE_COMPLETE_SUCCESSFULLY())
        {
            bool saving = GET_IS_DISPLAYINGSAVEMESSAGE();

            if (saving)
            {
                if (!tempBool)
                {
                    tempBool = true;
                    return true;
                }
            }
            else
            {
                tempBool = false;
            }
        }

        return false;
    }

    /// <summary>
    /// Saves all the data to the custom save file.
    /// </summary>
    /// <returns><see langword="true"/> if successful. Otherwise, <see langword="false"/>.</returns>
    public bool Save()
    {
        if (Settings != null)
            return Settings.Save();

        return false;
    }

    public string GetValue(string key, string defaultValue = "")
    {
        if (Settings == null)
            return defaultValue;
        if (string.IsNullOrWhiteSpace(IVGenericGameStorage.ValidSaveName))
            return defaultValue;

        return Settings.GetValue(IVGenericGameStorage.ValidSaveName, key, defaultValue);
    }
    public bool SetValue(string key, string value)
    {
        if (Settings == null)
            return false;
        if (string.IsNullOrWhiteSpace(IVGenericGameStorage.ValidSaveName))
            return false;

        Settings.AddKeyToSection(IVGenericGameStorage.ValidSaveName, key);
        return Settings.SetValue(IVGenericGameStorage.ValidSaveName, key, value);
    }

    #region Boolean
    public bool GetBoolean(string key, bool defaultValue = false)
    {
        if (Settings == null)
            return defaultValue;
        if (string.IsNullOrWhiteSpace(IVGenericGameStorage.ValidSaveName))
            return defaultValue;

        return Settings.GetBoolean(IVGenericGameStorage.ValidSaveName, key, defaultValue);
    }
    public bool SetBoolean(string key, bool value)
    {
        if (Settings == null)
            return false;
        if (string.IsNullOrWhiteSpace(IVGenericGameStorage.ValidSaveName))
            return false;

        Settings.AddKeyToSection(IVGenericGameStorage.ValidSaveName, key);
        return Settings.SetBoolean(IVGenericGameStorage.ValidSaveName, key, value);
    }
    #endregion

    #region Integer
    public int GetInteger(string key, int defaultValue = 0)
    {
        if (Settings == null)
            return defaultValue;
        if (string.IsNullOrWhiteSpace(IVGenericGameStorage.ValidSaveName))
            return defaultValue;

        return Settings.GetInteger(IVGenericGameStorage.ValidSaveName, key, defaultValue);
    }
    public bool SetInteger(string key, int value)
    {
        if (Settings == null)
            return false;
        if (string.IsNullOrWhiteSpace(IVGenericGameStorage.ValidSaveName))
            return false;

        Settings.AddKeyToSection(IVGenericGameStorage.ValidSaveName, key);
        return Settings.SetInteger(IVGenericGameStorage.ValidSaveName, key, value);
    }
    #endregion

    #region Float
    public float GetFloat(string key, float defaultValue = 0.0f)
    {
        if (Settings == null)
            return defaultValue;
        if (string.IsNullOrWhiteSpace(IVGenericGameStorage.ValidSaveName))
            return defaultValue;

        return Settings.GetFloat(IVGenericGameStorage.ValidSaveName, key, defaultValue);
    }
    public bool SetFloat(string key, float value)
    {
        if (Settings == null)
            return false;
        if (string.IsNullOrWhiteSpace(IVGenericGameStorage.ValidSaveName))
            return false;

        Settings.AddKeyToSection(IVGenericGameStorage.ValidSaveName, key);
        return Settings.SetFloat(IVGenericGameStorage.ValidSaveName, key, value);
    }
    #endregion

    #region Color
    public Color GetColor(string key, Color defaultValue = default(Color))
    {
        if (Settings == null)
            return defaultValue;
        if (string.IsNullOrWhiteSpace(IVGenericGameStorage.ValidSaveName))
            return defaultValue;

        return Settings.GetColor(IVGenericGameStorage.ValidSaveName, key, defaultValue);
    }
    public bool SetColor(string key, Color value)
    {
        if (Settings == null)
            return false;
        if (string.IsNullOrWhiteSpace(IVGenericGameStorage.ValidSaveName))
            return false;

        Settings.AddKeyToSection(IVGenericGameStorage.ValidSaveName, key);
        return Settings.SetColor(IVGenericGameStorage.ValidSaveName, key, value);
    }
    #endregion

    #region Key
    public Keys GetKey(string key, Keys defaultValue = Keys.None)
    {
        if (Settings == null)
            return defaultValue;
        if (string.IsNullOrWhiteSpace(IVGenericGameStorage.ValidSaveName))
            return defaultValue;

        return Settings.GetKey(IVGenericGameStorage.ValidSaveName, key, defaultValue);
    }
    public bool SetKey(string key, Keys value)
    {
        if (Settings == null)
            return false;
        if (string.IsNullOrWhiteSpace(IVGenericGameStorage.ValidSaveName))
            return false;

        Settings.AddKeyToSection(IVGenericGameStorage.ValidSaveName, key);
        return Settings.SetKey(IVGenericGameStorage.ValidSaveName, key, value);
    }
    #endregion

    #region Quaternion
    public Quaternion GetQuaternion(string key, Quaternion defaultValue = default(Quaternion))
    {
        if (Settings == null)
            return defaultValue;
        if (string.IsNullOrWhiteSpace(IVGenericGameStorage.ValidSaveName))
            return defaultValue;

        return Settings.GetQuaternion(IVGenericGameStorage.ValidSaveName, key, defaultValue);
    }
    public bool SetQuaternion(string key, Quaternion value)
    {
        if (Settings == null)
            return false;
        if (string.IsNullOrWhiteSpace(IVGenericGameStorage.ValidSaveName))
            return false;

        Settings.AddKeyToSection(IVGenericGameStorage.ValidSaveName, key);
        return Settings.SetQuaternion(IVGenericGameStorage.ValidSaveName, key, value);
    }
    #endregion

    #region Vector2
    public Vector2 GetVector2(string key, Vector2 defaultValue = default(Vector2))
    {
        if (Settings == null)
            return defaultValue;
        if (string.IsNullOrWhiteSpace(IVGenericGameStorage.ValidSaveName))
            return defaultValue;

        return Settings.GetVector2(IVGenericGameStorage.ValidSaveName, key, defaultValue);
    }
    public bool SetVector2(string key, Vector2 value)
    {
        if (Settings == null)
            return false;
        if (string.IsNullOrWhiteSpace(IVGenericGameStorage.ValidSaveName))
            return false;

        Settings.AddKeyToSection(IVGenericGameStorage.ValidSaveName, key);
        return Settings.SetVector2(IVGenericGameStorage.ValidSaveName, key, value);
    }
    #endregion

    #region Vector3
    public Vector3 GetVector3(string key, Vector3 defaultValue = default(Vector3))
    {
        if (Settings == null)
            return defaultValue;
        if (string.IsNullOrWhiteSpace(IVGenericGameStorage.ValidSaveName))
            return defaultValue;

        return Settings.GetVector3(IVGenericGameStorage.ValidSaveName, key, defaultValue);
    }
    public bool SetVector3(string key, Vector3 value)
    {
        if (Settings == null)
            return false;
        if (string.IsNullOrWhiteSpace(IVGenericGameStorage.ValidSaveName))
            return false;

        Settings.AddKeyToSection(IVGenericGameStorage.ValidSaveName, key);
        return Settings.SetVector3(IVGenericGameStorage.ValidSaveName, key, value);
    }
    #endregion

    #region Vector4
    public Vector4 GetVector4(string key, Vector4 defaultValue = default(Vector4))
    {
        if (Settings == null)
            return defaultValue;
        if (string.IsNullOrWhiteSpace(IVGenericGameStorage.ValidSaveName))
            return defaultValue;

        return Settings.GetVector4(IVGenericGameStorage.ValidSaveName, key, defaultValue);
    }
    public bool SetVector4(string key, Vector4 value)
    {
        if (Settings == null)
            return false;
        if (string.IsNullOrWhiteSpace(IVGenericGameStorage.ValidSaveName))
            return false;

        Settings.AddKeyToSection(IVGenericGameStorage.ValidSaveName, key);
        return Settings.SetVector4(IVGenericGameStorage.ValidSaveName, key, value);
    }
    #endregion

}