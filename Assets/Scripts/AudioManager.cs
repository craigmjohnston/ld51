namespace Oatsbarley.LD51
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using Random = UnityEngine.Random;

    [Serializable]
    public class AudioFile
    {
        public string Tag;
        public AudioClip Clip;
    }

    public class AudioManager : MonoBehaviour
    {
        [SerializeField] private AudioFile[] files;
        [SerializeField] private AudioSource audioSource;

        private static AudioManager instance;
        public static AudioManager Instance => AudioManager.instance;

        private Dictionary<string, List<AudioFile>> filesByTag;

        private void Awake()
        {
            AudioManager.instance = this;
        }

        private void Start()
        {
            this.filesByTag = new Dictionary<string, List<AudioFile>>();
            foreach (var audioFile in files)
            {
                if (!this.filesByTag.TryGetValue(audioFile.Tag, out List<AudioFile> list))
                {
                    list = new List<AudioFile>();
                    this.filesByTag[audioFile.Tag] = list;
                }

                list.Add(audioFile);
            }
        }

        public static void PlayOnce(string tag)
        {
            if (!Instance.filesByTag.ContainsKey(tag))
            {
                return;
            }

            var list = Instance.filesByTag[tag];
            var file = list[Random.Range(0, list.Count)];

            Instance.audioSource.PlayOneShot(file.Clip);
        }
    }
}