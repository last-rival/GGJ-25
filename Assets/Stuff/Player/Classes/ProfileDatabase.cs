using UnityEngine;

[CreateAssetMenu(fileName = "Profile ", menuName = "Custom/Profile Holder")]
public class ProfileDatabase  : ScriptableObject {
    public Profile defaultProfile;

    public Profile[] profiles;

    public Profile GetProfileByName(string name) {
        foreach (var profile in profiles) {
            if (profile.Name.Equals(name)) {
                return profile;
            }
        }

        return defaultProfile;
    }

}