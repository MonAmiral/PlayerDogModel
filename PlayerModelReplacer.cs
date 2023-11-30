using GameNetcodeStuff;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine;
using UnityEngine.Animations;

namespace PlayerDogModel
{
	public class PlayerModelReplacer : MonoBehaviour
	{
		private void Start()
		{
			// Disable LOD and renderers.
			this.GetComponentInChildren<LODGroup>().enabled = false;
			foreach (SkinnedMeshRenderer renderer in this.GetComponentsInChildren<SkinnedMeshRenderer>())
			{
				renderer.enabled = false;
			}

			// Disable level & beta tags.
			foreach (MeshRenderer renderer in this.GetComponentsInChildren<MeshRenderer>())
			{
				if (renderer.name == "BetaBadge" || renderer.name == "LevelSticker")
				{
					renderer.enabled = false;
				}
			}

			// Load and spawn new model.
			GameObject modelPrefab = LC_API.BundleAPI.BundleLoader.GetLoadedAsset<GameObject>("assets/Dog.fbx");
			GameObject modelInstance = Instantiate(modelPrefab, this.transform);
			modelInstance.transform.position = this.transform.position;
			modelInstance.transform.eulerAngles = this.transform.eulerAngles;
			modelInstance.transform.localScale *= 2f;

			// Copy the material.
			SkinnedMeshRenderer LOD1 = gameObject.GetComponent<PlayerControllerB>().thisPlayerModel;
			foreach (SkinnedMeshRenderer renderer in modelInstance.GetComponentsInChildren<SkinnedMeshRenderer>())
			{
				renderer.material = LOD1.material;
			}

			// Set up the anim correspondence with Constraints.
			Transform dogTorso = modelInstance.transform.Find("Armature").Find("torso");
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
	}
}

