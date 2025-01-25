using UnityEngine;

[CreateAssetMenu(fileName = "Profile ", menuName = "Custom/Profile Holder")]
public class ProfileDatabase  : ScriptableObject {
    public Profile defaultProfile;

    public Profile[] profiles;

    public Profile GetProfileOfType(ClassType classType) {
        foreach (var profile in profiles) {
            if (profile.id.Equals(classType)) {
                return profile;
            }
        }

        return defaultProfile;
    }

    public Profile GetProfileByName(string name) {
        foreach (var profile in profiles) {
            if (profile.Name.Equals(name)) {
                return profile;
            }
        }

        return defaultProfile;
    }

}