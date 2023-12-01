using GameNetcodeStuff;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine;
using UnityEngine.Animations;
using System.Threading.Tasks;
using UnityEngine.Networking;
using System.IO;
using System.Reflection;
using System.Collections;
using LC_API.ServerAPI;
using Newtonsoft.Json;

namespace PlayerDogModel
{
	public class PlayerModelReplacer : MonoBehaviour
	{
		private PlayerControllerB playerController;
		private GameObject dogGameObject;
		private LODGroup lodGroup;

		private AudioClip humanClip, dogClip;

		private Vector3 humanCameraPosition;

		private void Start()
		{
			this.playerController = this.GetComponent<PlayerControllerB>();
			this.humanCameraPosition = this.playerController.gameplayCamera.transform.localPosition;

			this.lodGroup = this.GetComponentInChildren<LODGroup>();

			this.SpawnDogModel();
			this.EnableHumanModel(false);

			this.humanClip = LC_API.BundleAPI.BundleLoader.GetLoadedAsset<AudioClip>("assets/changesuittohuman.wav");
			this.dogClip = LC_API.BundleAPI.BundleLoader.GetLoadedAsset<AudioClip>("assets/changesuittodog.wav");

			this.StartCoroutine(this.RetrieveAudio());

			Networking.GetString += this.Networking_GetString;
		}

		private void Update()
		{
			// Adjust camera height.
			Vector3 cameraPositionGoal = this.humanCameraPosition;
			if (this.dogGameObject.activeSelf)
			{
				if (!this.playerController.isCrouching)
				{
					cameraPositionGoal = new Vector3(0, -1.1f, 0.3f);
				}
				else
				{
					cameraPositionGoal = new Vector3(0, -0.5f, 0.3f);
				}
			}

			this.playerController.gameplayCamera.transform.localPosition = Vector3.MoveTowards(this.playerController.gameplayCamera.transform.localPosition, cameraPositionGoal, Time.deltaTime * 2);
		}

		private void SpawnDogModel()
		{
			// Load and spawn new model.
			GameObject modelPrefab = LC_API.BundleAPI.BundleLoader.GetLoadedAsset<GameObject>("assets/Dog.fbx");
			this.dogGameObject = Instantiate(modelPrefab, this.transform);
			this.dogGameObject.transform.position = this.transform.position;
			this.dogGameObject.transform.eulerAngles = this.transform.eulerAngles;
			this.dogGameObject.transform.localScale *= 2f;

			// Copy the material.
			SkinnedMeshRenderer LOD1 = this.playerController.thisPlayerModel;
			foreach (SkinnedMeshRenderer renderer in this.dogGameObject.GetComponentsInChildren<SkinnedMeshRenderer>())
			{
				renderer.material = LOD1.material;
			}

			// Set up the anim correspondence with Constraints.
			Transform dogTorso = this.dogGameObject.transform.Find("Armature").Find("torso");
			Transform dogHead = dogTorso.Find("head");
			Transform dogArmL = dogTorso.Find("arm.L");
			Transform dogArmR = dogTorso.Find("arm.R");
			Transform dogLegL = dogTorso.Find("butt").Find("leg.L");
			Transform dogLegR = dogTorso.Find("butt").Find("leg.R");

			Transform humanPelvis = this.transform.Find("ScavengerModel").Find("metarig").Find("spine");
			Transform humanHead = humanPelvis.Find("spine.001").Find("spine.002").Find("spine.003").Find("spine.004");
			Transform humanLegL = humanPelvis.Find("thigh.L");
			Transform humanLegR = humanPelvis.Find("thigh.R");

			// Add Constraints.
			// Note: the rotation offsets are not set because the model starts with the same rotation as the associated bones. Gotta add actual animations.
			PositionConstraint torsoConstraint = dogTorso.gameObject.AddComponent<PositionConstraint>();
			torsoConstraint.AddSource(new ConstraintSource() { sourceTransform = humanPelvis, weight = 1 });
			torsoConstraint.translationAtRest = dogTorso.localPosition;
			torsoConstraint.translationOffset = dogTorso.InverseTransformPoint(humanPelvis.position);
			torsoConstraint.constraintActive = true;
			torsoConstraint.locked = true;

			RotationConstraint headConstraint = dogHead.gameObject.AddComponent<RotationConstraint>();
			headConstraint.AddSource(new ConstraintSource() { sourceTransform = humanHead, weight = 1 });
			headConstraint.rotationAtRest = dogHead.localEulerAngles;
			headConstraint.constraintActive = true;
			headConstraint.locked = true;

			RotationConstraint armLConstraint = dogArmL.gameObject.AddComponent<RotationConstraint>();
			armLConstraint.AddSource(new ConstraintSource() { sourceTransform = humanLegR, weight = 1 });
			armLConstraint.rotationAtRest = dogArmL.localEulerAngles;
			armLConstraint.constraintActive = true;
			armLConstraint.locked = true;

			RotationConstraint armRConstraint = dogArmR.gameObject.AddComponent<RotationConstraint>();
			armRConstraint.AddSource(new ConstraintSource() { sourceTransform = humanLegL, weight = 1 });
			armRConstraint.rotationAtRest = dogArmR.localEulerAngles;
			armRConstraint.constraintActive = true;
			armRConstraint.locked = true;

			RotationConstraint legLConstraint = dogLegL.gameObject.AddComponent<RotationConstraint>();
			legLConstraint.AddSource(new ConstraintSource() { sourceTransform = humanLegL, weight = 1 });
			legLConstraint.rotationAtRest = dogLegL.localEulerAngles;
			legLConstraint.constraintActive = true;
			legLConstraint.locked = true;

			RotationConstraint legRConstraint = dogLegR.gameObject.AddComponent<RotationConstraint>();
			legRConstraint.AddSource(new ConstraintSource() { sourceTransform = humanLegR, weight = 1 });
			legRConstraint.rotationAtRest = dogLegR.localEulerAngles;
			legRConstraint.constraintActive = true;
			legRConstraint.locked = true;
		}

		private void EnableHumanModel(bool playAudio = true)
		{
			// Dog can be completely disabled because it doesn't drive the animations and sounds and other stuff.
			this.dogGameObject.SetActive(false);

			// Human renderers have to be directly disabled: the game object contains the camera and stuff and must remain enabled.
			this.lodGroup.enabled = true;
			this.playerController.thisPlayerModel.enabled = true;
			this.playerController.thisPlayerModelLOD1.enabled = true;
			this.playerController.thisPlayerModelLOD2.enabled = true;

			this.playerController.playerBetaBadgeMesh.gameObject.SetActive(true);
			this.playerController.playerBetaBadgeMesh.transform.parent.Find("LevelSticker").gameObject.SetActive(true);

			if (playAudio)
			{
				this.playerController.movementAudio.PlayOneShot(this.humanClip);
			}
		}

		private void EnableDogModel(bool playAudio = true)
		{
			this.dogGameObject.SetActive(true);
			this.dogGameObject.GetComponentInChildren<Renderer>().shadowCastingMode = this.playerController.thisPlayerModel.shadowCastingMode;

			this.lodGroup.enabled = false;
			this.playerController.thisPlayerModel.enabled = false;
			this.playerController.thisPlayerModelLOD1.enabled = false;
			this.playerController.thisPlayerModelLOD2.enabled = false;

			this.playerController.playerBetaBadgeMesh.gameObject.SetActive(false);
			this.playerController.playerBetaBadgeMesh.transform.parent.Find("LevelSticker").gameObject.SetActive(false);

			if (playAudio)
			{
				this.playerController.movementAudio.PlayOneShot(this.dogClip);
			}
		}

		public void ToggleAndBroadcast()
		{
			if (this.dogGameObject.activeSelf)
			{
				this.EnableHumanModel();
			}
			else
			{
				this.EnableDogModel();
			}

			ToggleData data = new ToggleData()
			{
				owner = this.playerController.playerUsername,
				isDog = this.dogGameObject.activeSelf,
			};

			Networking.Broadcast(JsonConvert.SerializeObject(data), "playerdogmodel");
		}

		private void Networking_GetString(string data, string signature)
		{
			if (signature != "playerdogmodel")
			{
				return;
			}

			ToggleData toggleData = JsonConvert.DeserializeObject<ToggleData>(data);
			if (this.playerController.playerUsername == toggleData.owner)
			{
				if (toggleData.isDog)
				{
					this.EnableDogModel(true);
				}
				else
				{
					this.EnableHumanModel(true);
				}
			}
		}

		private IEnumerator RetrieveAudio()
		{
			string fullPath = GetAssemblyFullPath("ChangeSuitToHuman.wav");
			Debug.Log(fullPath);
			UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(fullPath, AudioType.WAV);
			yield return request.SendWebRequest();
			if (request.error == null)
			{
				this.humanClip = DownloadHandlerAudioClip.GetContent(request);
				this.humanClip.name = Path.GetFileName(fullPath);
			}

			fullPath = GetAssemblyFullPath("ChangeSuitToDog.wav");
			request = UnityWebRequestMultimedia.GetAudioClip(fullPath, AudioType.WAV);
			yield return request.SendWebRequest();
			if (request.error == null)
			{
				this.dogClip = DownloadHandlerAudioClip.GetContent(request);
				this.dogClip.name = Path.GetFileName(fullPath);
			}
		}

		private static string GetAssemblyFullPath(string additionalPath)
		{
			string directoryName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			string path = ((additionalPath != null) ? Path.Combine(directoryName, ".\\" + additionalPath) : directoryName);
			return Path.GetFullPath(path);
		}

		[JsonObject]
		internal class ToggleData
		{
			[JsonProperty]
			public string owner
			{
				get;
				set;
			}

			[JsonProperty]
			public bool isDog
			{
				get;
				set;
			}
		}
	}
}

