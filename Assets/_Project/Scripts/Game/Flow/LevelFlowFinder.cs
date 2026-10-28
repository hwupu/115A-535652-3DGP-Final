using UnityEngine;

namespace Project3DGP.Game.Flow
{
    /// <summary>
    /// Finds the level's ILevelFlow for the UI (#20): the assigned component if it implements ILevelFlow, else the
    /// first one in the scene. So the HUD and menus connect to LevelManager (#27) as soon as it is in the scene,
    /// without re-wiring. Returns null if the scene has none yet.
    /// </summary>
    public static class LevelFlowFinder
    {
        public static ILevelFlow Find(MonoBehaviour assigned)
        {
            if (assigned is ILevelFlow flow)
                return flow;
            if (assigned != null)
                Debug.LogWarning($"{assigned.GetType().Name} is not an ILevelFlow; looking for one in the scene.", assigned);
            foreach (var behaviour in Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None))
                if (behaviour is ILevelFlow found)
                    return found;
            return null;
        }
    }
}
