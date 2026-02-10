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
        Button startGameButton;

        Label statusLabel;
        Label sprintHintLabel;
        Label titleLabel;

        // Relay UI
        TextField joinCodeField;
        Button joinWithCodeButton;
        Label joinCodeLabel;

        RelayConnector relay; // <-- add RelayConnector script somewhere in the scene

        void OnEnable()
        {
            relay = FindFirstObjectByType<RelayConnector>();

            var uiDocument = GetComponent<UIDocument>();
            rootVisualElement = uiDocument.rootVisualElement;

            hostButton = CreateButton("HostButton", "Host (Relay)");
            clientButton = CreateButton("ClientButton", "Client (No Relay)");
            serverButton = CreateButton("ServerButton", "Server (No Relay)");
            startGameButton = CreateButton("StartGameButton", "Start Game");

            statusLabel = CreateLabel("StatusLabel", "Not Connected");
            //titleLabel = CreateLabel("TitleLabel", "TAG");
            sprintHintLabel = CreateLabel("SprintHintLabel", "Hold SPACE to sprint briefly");

            // Relay join-code UI
            joinCodeField = new TextField("Join Code");
            joinCodeField.name = "JoinCodeField";
            joinCodeField.style.width = 240;

            joinWithCodeButton = CreateButton("JoinWithCodeButton", "Join With Code (Relay)");

            joinCodeLabel = CreateLabel("JoinCodeLabel", "Join Code: (none)");
            joinCodeLabel.style.unityFontStyleAndWeight = FontStyle.Bold;

            // TAG title (top center)
            //titleLabel.style.position = Position.Absolute;
            //titleLabel.style.top = 8;
            //titleLabel.style.left = 0;
            //titleLabel.style.right = 0;
            //titleLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            //titleLabel.style.fontSize = 36;
            //titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;

            // Sprint hint (top right)
            sprintHintLabel.style.position = Position.Absolute;
            sprintHintLabel.style.top = 12;
            sprintHintLabel.style.right = 12;
            sprintHintLabel.style.fontSize = 14;
            sprintHintLabel.style.color = new Color(0.25f, 0.25f, 0.25f);

            rootVisualElement.Clear();

            // Connection buttons
            rootVisualElement.Add(hostButton);

            // Relay join controls
            rootVisualElement.Add(joinCodeField);
            rootVisualElement.Add(joinWithCodeButton);
            rootVisualElement.Add(joinCodeLabel);

            // Optional legacy buttons (direct LAN / direct IP etc.)
            rootVisualElement.Add(clientButton);
            rootVisualElement.Add(serverButton);

            // In-game
            rootVisualElement.Add(startGameButton);
            rootVisualElement.Add(statusLabel);

            // Overlays
            //rootVisualElement.Add(titleLabel);
            rootVisualElement.Add(sprintHintLabel);

            hostButton.clicked += OnHostButtonClicked;
            joinWithCodeButton.clicked += OnJoinWithCodeClicked;
            clientButton.clicked += OnClientButtonClicked;
            serverButton.clicked += OnServerButtonClicked;
            startGameButton.clicked += OnStartGameClicked;
        }

        void Update()
        {
            UpdateUI();
        }

        void OnDisable()
        {
            hostButton.clicked -= OnHostButtonClicked;
            joinWithCodeButton.clicked -= OnJoinWithCodeClicked;
            clientButton.clicked -= OnClientButtonClicked;
            serverButton.clicked -= OnServerButtonClicked;
            startGameButton.clicked -= OnStartGameClicked;
        }

        async void OnHostButtonClicked()
        {
            if (relay == null)
            {
                Debug.LogError("RelayConnector not found in scene. Add RelayConnector to a GameObject.");
                return;
            }

            try
            {
                SetStatusText("Starting Host (Relay)...");
                string code = await relay.StartHostWithRelay();
                joinCodeLabel.text = $"Join Code: {code}";
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                joinCodeLabel.text = "Join Code: (failed)";
                SetStatusText("Failed to start Host (Relay)");
            }
        }

        async void OnJoinWithCodeClicked()
        {
            if (relay == null)
            {
                Debug.LogError("RelayConnector not found in scene. Add RelayConnector to a GameObject.");
                return;
            }

            string code = joinCodeField.value?.Trim();
            if (string.IsNullOrEmpty(code))
            {
                Debug.LogWarning("Enter a join code first.");
                return;
            }

            try
            {
                SetStatusText("Joining (Relay)...");
                await relay.StartClientWithRelay(code);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                SetStatusText("Failed to join (Relay)");
            }
        }

        // Optional: keep your original direct-start buttons for local testing / future IP transport work
        void OnClientButtonClicked() => NetworkManager.Singleton.StartClient();
        void OnServerButtonClicked() => NetworkManager.Singleton.StartServer();

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
                startGameButton.style.display = DisplayStyle.None;
                SetStatusText("NetworkManager not found");
                return;
            }

            bool connected = NetworkManager.Singleton.IsClient || NetworkManager.Singleton.IsServer;

            if (!connected)
            {
                SetStartButtons(true);

                // show relay join UI when not connected
                joinCodeField.style.display = DisplayStyle.Flex;
                joinWithCodeButton.style.display = DisplayStyle.Flex;
                joinCodeLabel.style.display = DisplayStyle.Flex;

                startGameButton.style.display = DisplayStyle.None;
                SetStatusText("Not connected");
            }
            else
            {
                SetStartButtons(false);

                // hide connection UI when connected
                joinCodeField.style.display = DisplayStyle.None;
                joinWithCodeButton.style.display = DisplayStyle.None;
                // keep join code label visible for host so they can read it out; hide for clients
                joinCodeLabel.style.display = NetworkManager.Singleton.IsHost ? DisplayStyle.Flex : DisplayStyle.None;

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

            // These are optional "non-relay" buttons you had before; keep or remove.
            clientButton.style.display = state ? DisplayStyle.Flex : DisplayStyle.None;
            serverButton.style.display = state ? DisplayStyle.Flex : DisplayStyle.None;
        }

        void SetStatusText(string text) => statusLabel.text = text;

        void UpdateStatusLabels()
        {
            var mode = NetworkManager.Singleton.IsHost ? "Host"
                : NetworkManager.Singleton.IsServer ? "Server"
                : "Client";

            string transport = "Transport: " + NetworkManager.Singleton.NetworkConfig.NetworkTransport.GetType().Name;
            string modeText = "Mode: " + mode;

            SetStatusText($"{transport}\n{modeText}");
        }
    }
}