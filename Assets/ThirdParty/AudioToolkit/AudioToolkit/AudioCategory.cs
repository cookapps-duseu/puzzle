using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace CS.AudioToolkit
{
    /// <summary>
    /// An audio category represents a set of AudioItems. Categories allow to change the volume of all containing audio items.
    /// </summary>
    [System.Serializable]
    public class AudioCategory
    {
        public enum PlayWithZeroVolumeOptions
        {
            DefaultFromAudioController = 0,
            On = 1,
            Off = 2,
        }
        /// <summary>
        /// The name of category ( = <c>categoryID</c> )
        /// </summary>
        public string Name;

        /// <summary>
        /// The volume factor applied to all audio items in the category (NOT including a possible <see cref="parentCategory"/>)
        /// If you change the volume by script the change will be applied to all 
        /// playing audios immediately.
        /// </summary>
        public float Volume
        {
            get { return _volume; }
            set
            {
                _volume = value;
                _ApplyVolumeChange(); //TODO:  maybe call to _ApplyVolumeChange not necessary anymore since change to AudioObject._UpdateFadeVolume
            }
        }

        /// <summary>
        /// The volume factor applied to all audio items in the category (including a possible <see cref="parentCategory"/> and fade out/in)
        /// </summary>
        public float VolumeTotal
        {
            get
            {
                _UpdateFadeTime();
                float fadeVal = audioFader.Get();
                if( parentCategory != null )
                {
                    return parentCategory.VolumeTotal * _volume * fadeVal;
                }
                else
                    return _volume * fadeVal;
            }
        }

        /// <summary>
        /// If a parent category is specified, the category inherits the volume of its parent.
        /// </summary>
        public AudioCategory parentCategory
        {
            set
            {
                _parentCategory = value;

                if( value != null )
                {
                    _parentCategoryName = _parentCategory.Name;
                }
                else
                    _parentCategoryName = null;
            }
            get
            {
                if( string.IsNullOrEmpty( _parentCategoryName ) )
                {
                    return null;
                }
                if( _parentCategory == null )
                {
                    if( audioController != null )
                    {
                        _parentCategory = audioController._GetCategory( _parentCategoryName );
                    }
                    else
                    {
                        Debug.LogWarning( "_audioController == null" );
                    }
                }
                return _parentCategory;
            }
        }

        private AudioCategory _parentCategory;
        private AudioFader _audioFader;
        private AudioFader audioFader
        {
            get
            {
                if( _audioFader == null )
                {
                    _audioFader = new AudioFader();
                }
                return _audioFader;
            }
        }

        [SerializeField]
        private string _parentCategoryName;

        /// <summary>
        /// If disabled, audios in this category are not played if they have a resulting volume of zero.
        /// </summary>
        public PlayWithZeroVolumeOptions playWithZeroVolume;

        /// <summary>
        /// The <see cref="AudioController"/> the category belongs to
        /// </summary>
        public AudioController audioController { get; set; }

        /// <summary>
        /// Allows to define a specific audio object prefab for this category. If none is defined, 
        /// the default prefab as set by <see cref="AudioController.AudioObjectPrefab"/> is taken.
        /// </summary>
        /// <remarks> This way you can e.g. use special effects such as the reverb filter for 
        /// a specific category. Just add the respective filter component to the specified prefab.</remarks>
        public GameObject AudioObjectPrefab;

        /// <summary>
        /// Define your AudioItems using Unity inspector.
        /// </summary>  
        public AudioItem[] AudioItems;

        [SerializeField]
        private float _volume = 1.0f;

        /// <summary>
        /// Allows to assign the category to a Unity 5 Audio Mixer Group
        /// </summary>
        public AudioMixerGroup audioMixerGroup;

        /// <summary>
        /// Instantiates an AudioCategory
        /// </summary>
        /// <param name="audioController">The <see cref="AudioController"/> the category belongs to.</param>
        public AudioCategory( AudioController audioController )
        {
            this.audioController = audioController;
        }

        /// <summary>
        /// Adds an AudioItem to this category
        /// </summary>
        /// <param name="audioItem">The <see cref="AudioItem"/> to add to the category.</param>
        public void AddAudioItem( AudioItem audioItem )
        {
            ArrayHelper.AddArrayElement( ref AudioItems, audioItem );
            audioItem.category = this;
            if( audioController ) audioController._InvalidateCategories();
        }

        /// <summary>
        /// Retrieves the AudioObjectPrefab associated with this category. AudioObjectPrefabs are inherited by the parent category.
        /// </summary>
        /// <returns>
        /// The AudioObjectPrefab associated with this category.
        /// </returns>
        public GameObject GetAudioObjectPrefab()
        {
            if( AudioObjectPrefab != null )
                return AudioObjectPrefab;
            else
            {
                if( parentCategory != null )
                {
                    return parentCategory.GetAudioObjectPrefab();
                }
                else
                {
                    return audioController.AudioObjectPrefab;
                }
            }
        }

        /// <summary>
        /// Retrieves the AudioMixerGroup associated with this category. AudioMixerGroupa are inherited by the parent category.
        /// </summary>
        /// <returns>
        /// The AudioMixerGroup associated with this category.
        /// </returns>
        public AudioMixerGroup GetAudioMixerGroup()
        {
            if( audioMixerGroup != null )
                return audioMixerGroup;
            else
            {
                if( parentCategory != null )
                {
                    return parentCategory.GetAudioMixerGroup();
                }
                else
                {
                    return null;
                }
            }
        }

        internal void _AnalyseAudioItems( Dictionary<string, AudioItem> audioItemsDict )
        {
            if( AudioItems == null ) return;

            foreach( AudioItem ai in AudioItems )
            {
                if( ai != null )
                {
                    ai._Initialize( this );
#if AUDIO_TOOLKIT_DEMO
                int? demoMaxNumAudioItemsConst = 0x12345B;

                int? demoMaxNumAudioItems = (demoMaxNumAudioItemsConst & 0xf);
                demoMaxNumAudioItems++;

                if ( audioItemsDict.Count > demoMaxNumAudioItems )
                {
                    Debug.LogError( "Audio Toolkit: The demo version does not allow more than " + demoMaxNumAudioItems + " audio items." );
                    Debug.LogWarning( "Please buy the full version of Audio Toolkit!" );
                    return;
                }
#endif

                    //Debug.Log( string.Format( "SubItem {0}: {1} {2} {3}", fi.Name, ai.FixedOrder, ai.RandomOrderStart, ai._lastChosen ) );

                    if( audioItemsDict != null )
                    {
                        try
                        {
                            audioItemsDict.Add( ai.Name, ai );
                        }
                        catch( ArgumentException )
                        {
                            Debug.LogWarning( "Multiple audio items with name '" + ai.Name + "'", audioController );
                        }
                    }
                }

            }
        }

        internal int _GetIndexOf( AudioItem audioItem )
        {
            if( AudioItems == null ) return -1;

            for( int i = 0; i < AudioItems.Length; i++ )
            {
                if( audioItem == AudioItems[i] ) return i;
            }
            return -1;
        }

        private void _ApplyVolumeChange()
        {
            AudioController.InvokeForAllPlayingAudioObjects( ( o ) =>
            {
                if( _IsCategoryParentOf( o.category, this ) )
                {
                    o._ApplyVolumeBoth();
                }
            } );
        }

        bool _IsCategoryParentOf( AudioCategory toTest, AudioCategory parent )
        {
            var cat = toTest;
            while( cat != null )
            {
                if( cat == parent ) return true;
                cat = cat.parentCategory;
            }
            return false;
        }

        /// <summary>
        /// Unloads all AudioClips specified in the subitems from memory. 
        /// </summary>
        /// <remarks>
        /// You will still be able to play the AudioClips, but you may experience performance hickups when Unity reloads the audio asset
        /// </remarks>
        public void UnloadAllAudioClips()
        {
            for( int i = 0; i < AudioItems.Length; i++ )
            {
                AudioItems[i].UnloadAudioClip();
            }
        }


        /// <summary>
        /// Starts a fade-in of the audio category.
        /// </summary>
        /// <param name="fadeInTime">The fade time in seconds.</param>
        /// <param name="stopCurrentFadeOut">In case of an existing fade-out this parameter determines if the fade-out is stopped.</param>
        public void FadeIn( float fadeInTime, bool stopCurrentFadeOut = true )
        {
            _UpdateFadeTime();
            audioFader.FadeIn( fadeInTime, stopCurrentFadeOut );
        }

        /// <summary>
        /// Starts a fade-out of the audio category.
        /// </summary>
        /// <remarks>
        /// If the category is already fading out the requested fade-out is combined with the existing one.
        /// </remarks>
        /// <param name="fadeOutLength">The fade time in seconds. If a negative value is specified, the fade out as specified in the corresponding <see cref="AudioSubItem.FadeOut"/> is used</param>
        /// <param name="startToFadeTime">Fade out starts after <c>startToFadeTime</c> seconds have passed</param>
        public void FadeOut( float fadeOutLength, float startToFadeTime = 0 )
        {
            _UpdateFadeTime();
            audioFader.FadeOut( fadeOutLength, startToFadeTime );
        }

        private void _UpdateFadeTime()
        {
            audioFader.time = AudioController.systemTime;
        }

        /// <summary>
        /// return <c>true</c> if the category is currently fading in
        /// </summary>
        public bool isFadingIn
        {
            get
            {
                return audioFader.isFadingIn;
            }
        }

        /// <summary>
        /// return <c>true</c> if the category is currently fading out
        /// </summary>
        /// <remarks>
        /// If the fade-out is complete then <see cref="isFadingOut"/> return <c>false</c> and <see cref="isFadeOutComplete"/> returns <c>true</c>
        /// </remarks>
        public bool isFadingOut
        {
            get
            {
                return audioFader.isFadingOut;
            }
        }

        /// <summary>
        /// return <c>true</c> if the category has completely faded out
        /// </summary>
        public bool isFadeOutComplete
        {
            get
            {
                return audioFader.isFadingOutComplete;
            }
        }
    }

    /// <summary>
    /// Used by <see cref="AudioItem"/> to determine which <see cref="AudioSubItem"/> is chosen. 
    /// </summary>
    public enum AudioPickSubItemMode
    {
        /// <summary>disables playback</summary>  
        Disabled,

        /// <summary>chooses a random subitem with a probability in proportion to <see cref="AudioSubItem.Probability"/> </summary>  
        Random,

        /// <summary>chooses a random subitem with a probability in proportion to <see cref="AudioSubItem.Probability"/> and makes sure it is not played twice in a row (if possible)</summary>
        RandomNotSameTwice,

        /// <summary> chooses the subitems in a sequence one after the other starting with the first</summary>
        Sequence,

        /// <summary> chooses the subitems in a sequence one after the other starting with a random subitem</summary>
        SequenceWithRandomStart,

        /// <summary> chooses all subitems at the same time</summary>
        AllSimultaneously,

        /// <summary> chooses two different subitems at the same time (if possible)</summary>
        TwoSimultaneously,

        /// <summary> always chooses the first subitem. Intended to be used with with a <see cref="AudioItem.LoopMode"></see></summary>
        StartLoopSequenceWithFirst,

        /// <summary> Same as RandomNotSameTwice but only picks from odds or evens switching every time. Useful for footsteps left/right</summary>
        RandomNotSameTwiceOddsEvens,
    }
}