using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

namespace HelloWorld
{
    public class HelloWorldManager : MonoBehaviour
    {
        VisualElement rootVisualElement;
        Button hostButton;
        Button clientButton;
        Button serverButton;
        //Button moveButton;
        Button startGameButton;
        Label statusLabel;
        Label sprintHintLabel;
        Label titleLabel;

        void OnEnable()
        {
            var uiDocument = GetComponent<UIDocument>();
            rootVisualElement = uiDocument.rootVisualElement;

            hostButton = CreateButton("HostButton", "Host");
            clientButton = CreateButton("ClientButton", "Client");
            serverButton = CreateButton("ServerButton", "Server");
            //moveButton = CreateButton("MoveButton", "Move");
            startGameButton = CreateButton("StartGameButton", "Start Game");
            statusLabel = CreateLabel("StatusLabel", "Not Connected");
            titleLabel = CreateLabel("TitleLabel", "TAG");
            sprintHintLabel = CreateLabel("SprintHintLabel", "Hold SPACE to sprint briefly");

            // TAG title (top center)
            titleLabel.style.position = Position.Absolute;
            titleLabel.style.top = 8;
            titleLabel.style.left = 0;
            titleLabel.style.right = 0;
            titleLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            titleLabel.style.fontSize = 36;
            titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;

            // Sprint hint (top right)
            sprintHintLabel.style.position = Position.Absolute;
            sprintHintLabel.style.top = 12;
            sprintHintLabel.style.right = 12;
            sprintHintLabel.style.fontSize = 14;
            sprintHintLabel.style.color = new Color(0.25f, 0.25f, 0.25f);

            rootVisualElement.Clear();
            rootVisualElement.Add(hostButton);
            rootVisualElement.Add(clientButton);
            rootVisualElement.Add(serverButton);
            //rootVisualElement.Add(moveButton);
            rootVisualElement.Add(startGameButton);
            rootVisualElement.Add(statusLabel);
            rootVisualElement.Add(titleLabel);
            rootVisualElement.Add(sprintHintLabel);

            hostButton.clicked += OnHostButtonClicked;
            clientButton.clicked += OnClientButtonClicked;
            serverButton.clicked += OnServerButtonClicked;
            //moveButton.clicked += SubmitNewPosition;
            startGameButton.clicked += OnStartGameClicked;
        }

        void Update()
        {
            UpdateUI();
        }

        void OnDisable()
        {
            hostButton.clicked -= OnHostButtonClicked;
            clientButton.clicked -= OnClientButtonClicked;
            serverButton.clicked -= OnServerButtonClicked;
            //moveButton.clicked -= SubmitNewPosition;
            startGameButton.clicked -= OnStartGameClicked;
        }

        void OnHostButtonClicked() => NetworkManager.Singleton.StartHost();

        void OnClientButtonClicked() => NetworkManager.Singleton.StartClient();

        void OnServerButtonClicked() => NetworkManager.Singleton.StartServer();

        // Disclaimer: This is not the recommended way to create and stylize the UI elements, it is only utilized for the sake of simplicity.
        // The recommended way is to use UXML and USS. Please see this link for more information: https://docs.unity3d.com/Manual/UIE-USS.html
        private Button CreateButton(string name, string text)
        {
            var button = new Button();
            button.name = name;
            button.text = text;
            button.style.width = 240;
            button.style.backgroundColor = Color.white;
            button.style.color = Color.black;
            button.style.unityFontStyleAndWeight = FontStyle.Bold;
            return button;
        }

        private Label CreateLabel(string name, string content)
        {
            var label = new Label();
            label.name = name;
            label.text = content;
            label.style.color = Color.black;
            label.style.fontSize = 18;
            return label;
        }

        void UpdateUI()
        {
            if (NetworkManager.Singleton == null)
            {
                SetStartButtons(false);
                //SetMoveButton(false);
                startGameButton.style.display = DisplayStyle.None;
                SetStatusText("NetworkManager not found");
                return;
            }

            if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
            {
                SetStartButtons(true);
                //SetMoveButton(false);
                startGameButton.style.display = DisplayStyle.None;
                SetStatusText("Not connected");
            }
            else
            {
                SetStartButtons(false);
                //SetMoveButton(true);
                UpdateStatusLabels();
                UpdateStartGameButton(); 
            }

        }

        void UpdateStartGameButton()
        {
            if (NetworkManager.Singleton.IsHost)
            {
                bool enoughPlayers = NetworkManager.Singleton.ConnectedClientsIds.Count >= 2;
                startGameButton.style.display = enoughPlayers ? DisplayStyle.Flex : DisplayStyle.None;
            }
            else
            {
                startGameButton.style.display = DisplayStyle.None;
            }
        }

        void OnStartGameClicked()
        {
            if (NetworkManager.Singleton.IsHost)
            {
                TagGameManager.Instance.StartGameServerRpc();
            }
        }


        void SetStartButtons(bool state)
        {
            hostButton.style.display = state ? DisplayStyle.Flex : DisplayStyle.None;
            clientButton.style.display = state ? DisplayStyle.Flex : DisplayStyle.None;
            serverButton.style.display = state ? DisplayStyle.Flex : DisplayStyle.None;
        }

        //void SetMoveButton(bool state)
        //{
        //    moveButton.style.display = state ? DisplayStyle.Flex : DisplayStyle.None;
        //    if (state)
        //    {
        //        moveButton.text = NetworkManager.Singleton.IsServer ? "Move" : "Request Position Change";
        //    }
        //}

        void SetStatusText(string text) => statusLabel.text = text;

        void UpdateStatusLabels()
        {
            var mode = NetworkManager.Singleton.IsHost ? "Host" : NetworkManager.Singleton.IsServer ? "Server" : "Client";
            string transport = "Transport: " + NetworkManager.Singleton.NetworkConfig.NetworkTransport.GetType().Name;
            string modeText = "Mode: " + mode;
            SetStatusText($"{transport}\n{modeText}");
        }

        void SubmitNewPosition()
        {
            if (NetworkManager.Singleton.IsServer && !NetworkManager.Singleton.IsClient)
            {
                foreach (ulong uid in NetworkManager.Singleton.ConnectedClientsIds)
                {
                    var playerObject = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(uid);
                    var player = playerObject.GetComponent<HelloWorldPlayer>();
                    player.Move();
                }
            }
            else if (NetworkManager.Singleton.IsClient)
            {
                var playerObject = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
                var player = playerObject.GetComponent<HelloWorldPlayer>();
                player.Move();
            }
        }
    }
}