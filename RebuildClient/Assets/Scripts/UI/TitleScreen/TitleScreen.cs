using System;
using System.Collections.Generic;
using Assets.Scripts.Network;
using Assets.Scripts.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Assets.Scripts.UI.TitleScreen
{
    public class TitleScreen : MonoBehaviour
    {
        public enum TitleScreenState { LogIn, CharacterSelect, CharacterCreation, NoticeBox, Waiting }

        public TitleScreenState TitleState = TitleScreenState.LogIn;
        public TitleScreenState LastTitleState = TitleScreenState.LogIn;
        public List<Sprite> Backgrounds;
        public Image BackgroundArea;

        public LoginBox LoginBox;
        public CharacterSelectWindow CharacterSelectWindow;
        public CharacterCreatorWindow CharacterCreatorWindow;

        
        public TextMeshProUGUI NoticeBoxText;
        public GameObject NoticeBox;
        

        private bool isInErrorState;
        private float loginTimeoutTime;
        private bool isLoginTimerActive;

        [NonSerialized] public AudioClip ButtonSound;

        private const float timeoutLength = 30f;

        
        public void OpenCharacterSelect(List<ClientCharacterSummary> characters)
        {
            isLoginTimerActive = false;
            CharacterSelectWindow.gameObject.SetActive(true);
            TitleState = TitleScreenState.CharacterSelect;

            CharacterCreatorWindow.HidePane();
            CharacterSelectWindow.PrepareSelectWindow(this, characters);
        }

        public void StartLoginTimer()
        {
            loginTimeoutTime = Time.timeSinceLevelLoad + timeoutLength;
            isLoginTimerActive = true;
            LoginBox.gameObject.SetActive(false);
        }

        public void LogInError(string message)
        {
            Debug.Log($"LogInError: " + message);
            NoticeBox.gameObject.SetActive(true);
            LoginBox.gameObject.SetActive(false);
            CharacterSelectWindow.gameObject.SetActive(false);
            NoticeBoxText.text = message;
            isInErrorState = true;
            isLoginTimerActive = false;
            TitleState = TitleScreenState.LogIn; //we return to login state now
        }

        public void DisconnectAndReturnToLogin()
        {
            NetworkManager.Instance.Disconnect();
            LoginBox.gameObject.SetActive(true);
            NoticeBox.gameObject.SetActive(false);
            CharacterSelectWindow.gameObject.SetActive(false);
            isInErrorState = false;
            isLoginTimerActive = false;
            TitleState = TitleScreenState.LogIn;
        }

        public void ErrorMessage(string message)
        {
            if(TitleState != TitleScreenState.Waiting)
                LastTitleState = TitleState;
            TitleState = TitleScreenState.NoticeBox;
            NoticeBoxText.text = message;
            NoticeBox.gameObject.SetActive(true);
        }

        public void NoticeBoxOk()
        {
            isInErrorState = false;
            switch (TitleState)
            {
                case TitleScreenState.LogIn:
                    NoticeBox.gameObject.SetActive(false);
                    LoginBox.gameObject.SetActive(true);
                    return;
                case TitleScreenState.NoticeBox:
                    TitleState = LastTitleState;
                    NoticeBox.gameObject.SetActive(false);
                    return;
            }
        }
        
        // Start is called before the first frame update
        void Awake()
        {
            BackgroundArea.sprite = Backgrounds[Random.Range(0, Backgrounds.Count)];
            NoticeBox.gameObject.SetActive(false);
            CharacterSelectWindow.gameObject.SetActive(false);
            
            AddressableUtility.Load<AudioClip>(gameObject, "Assets/Sounds/Effects/버튼소리.ogg", a => ButtonSound = a);
        }

        // Update is called once per frame
        void Update()
        {
            if(isInErrorState && (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
                NoticeBoxOk();

            if (isLoginTimerActive && Time.timeSinceLevelLoad > loginTimeoutTime)
            {
                isLoginTimerActive = false;
                LogInError($"Unable to connect to server, the connection timed out.");
            }
        }
    }
}