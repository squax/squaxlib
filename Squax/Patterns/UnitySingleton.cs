using UnityEngine;

namespace Squax.Patterns
{
    /// <summary>
    /// Base class for Unity Singleton pattern management.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class UnitySingleton<T> : MonoBehaviour where T : MonoBehaviour
	{
		protected static T instance = null;

        protected static bool isDead = false;

		/// <summary>
		/// Unity singleton pattern; public interface for creation
		/// </summary>
		public static T Instance
		{
			get
			{
                if (isDead == true)
                {
                    return null;
                }

                if (instance == null)
                {
	                if(Application.isPlaying == false)
	                {
	                    return null;
	                }

                    instance = (T)FindObjectOfType(typeof(T));

                    if (instance == null)
                    {
                        var go = new GameObject(typeof(T).Name);
                        instance = go.AddComponent<T>();
                    }

                    DontDestroyOnLoad(instance.gameObject);
                }

                return instance;
			}
		}

        public static bool IsInstanciated()
        {
            return instance != null;
        }

		protected void OnAwake(string singletonName = "")
		{
            instance = GetComponent<T>();

            gameObject.name = (string.IsNullOrEmpty(singletonName) == true) ? "UnitySingleton<" + typeof(T).Name + ">" : singletonName;

            if(gameObject.transform.parent == null)
                DontDestroyOnLoad(gameObject);
		}

        void OnApplicationQuit()
        {
            isDead = true;
        }

        virtual protected void OnDestroy()
        {
            instance = null;

            if (isDead == false && Application.isPlaying == true)
            {
                Debug.LogWarning("Destroying UnitySingleton<" + typeof(T).Name + ">? If intentional, ignore this warning.");
            }
        }
	}
}

