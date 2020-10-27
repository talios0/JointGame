using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;


public enum Players
{
    STANDARD,
    DRONE
}

public class PlayerManager : MonoBehaviour
{
    private PlayerPlayState playState;

    [Header("Standard Components")]
    public Transform standard;
    public Rigidbody standardBody;
    public LargePlayerCamera standardCamera;
    public Camera attachedStandardCamera;
    public LargePlayerMovement standardMovement;

    [Header("Drone Componenets")]
    public Transform drone;
    public Rigidbody droneBody;
    public DroneCamera droneCamera;
    public Camera attachedDroneCamera;
    public DroneMovement droneMovement;
    public GameObject droneUI;

    [Header("Transition")]
    [Range(0, 1)]
    public float increment;
    [Range(0, 1)]
    public float weightTransferIncrement;
    public Volume transitionVolume;
    public VolumeProfile transitionStartProfile, standardProfile, droneProfile;
    public Transform transitionTransform;
    public Camera transitionCamera;

    private bool transition = false;

    private void Start()
    {
        ChangePlayerPlayState(PlayerPlayState.STANDARD);
    }

    public void Update()
    {
        if (Input.GetButtonDown("Switch") && !transition) ShiftPlayerPlayState();
    }

    public void ChangePlayerPlayState(PlayerPlayState state)
    {
        switch (state)
        {
            case PlayerPlayState.STANDARD:
                if (standardBody.velocity.y != 0) return;
                playState = state;
                TransitionToStandard();
                break;
            case PlayerPlayState.DRONE:
                if (droneBody.velocity.y != 0) return;
                playState = state;
                TransitionToDrone();
                break;
            default:
                Debug.LogWarning("No valid state for: " + state);
                break;
        }
    }

    public void ShiftPlayerPlayState()
    {
        switch (playState)
        {
            case PlayerPlayState.STANDARD:
                ChangePlayerPlayState(PlayerPlayState.DRONE);
                break;
            case PlayerPlayState.DRONE:
                ChangePlayerPlayState(PlayerPlayState.STANDARD);
                break;
            default:
                Debug.LogWarning("No valid state shift for: " + playState);
                break;
        }
    }

    public PlayerPlayState GetPlayState()
    {
        return playState;
    }

    private void TransitionToStandard()
    {
        if (transition) return;
        transition = true;
        transitionTransform.position = droneCamera.transform.position;
        transitionTransform.rotation = droneCamera.transform.rotation;
        // Disable
        droneMovement.DisableMovement();
        attachedDroneCamera.enabled = false;
        droneCamera.Disable();
        droneUI.SetActive(false);
        droneBody.isKinematic = true;
        droneBody.Sleep();

        transitionCamera.enabled = true;
        StartCoroutine(Transition(droneCamera.transform.position, standardCamera.transform.position, attachedDroneCamera, attachedStandardCamera, transitionStartProfile, standardProfile, Players.STANDARD));
    }

    private void TransitionToDrone()
    {
        if (transition) return;
        transition = true;
        transitionTransform.position = standardCamera.transform.position;
        transitionTransform.rotation = standardCamera.transform.rotation;
        // Disable
        standardMovement.DisableMovement();
        attachedStandardCamera.enabled = false;
        standardCamera.enabled = false;
        standardBody.isKinematic = true;
        standardBody.Sleep();

        transitionCamera.enabled = true;
        StartCoroutine(Transition(standardCamera.transform.position, droneCamera.transform.position, attachedStandardCamera, attachedDroneCamera, transitionStartProfile, droneProfile, Players.DRONE));
    }

    private void EndTransition()
    {
        switch (playState)
        {
            case PlayerPlayState.STANDARD:
                standardMovement.EnableMovement();
                attachedStandardCamera.enabled = true;
                standardCamera.enabled = true;
                standardBody.isKinematic = false;
                break;
            case PlayerPlayState.DRONE:
                droneMovement.EnableMovement();
                attachedDroneCamera.enabled = true;
                droneCamera.Enable();
                droneUI.SetActive(true);
                droneBody.isKinematic = false;
                break;
            default:
                Debug.LogWarning("Unknown End Transition State: " + playState);
                break;
        }
        transitionCamera.enabled = false;
        transitionVolume.enabled = false;
        transition = false;
    }

    IEnumerator Transition(Vector3 startPos, Vector3 finalPos, Camera startCam, Camera endCam, VolumeProfile startProfile, VolumeProfile endProfile, Players endPlayer)
    {
        transitionVolume.enabled = true;

        // Switch to more efficient method later
        Bloom[] bloom = new Bloom[3];
        ChromaticAberration[] chromaticAberration = new ChromaticAberration[3];
        LensDistortion[] lensDistortion = new LensDistortion[3];
        FilmGrain[] filmGrain = new FilmGrain[3];
        ShadowsMidtonesHighlights[] shadowsMidtonesHighlights = new ShadowsMidtonesHighlights[3];
        PaniniProjection[] paniniProjection = new PaniniProjection[3];
        WhiteBalance[] whiteBalance = new WhiteBalance[3];
        DepthOfField[] depthOfField = new DepthOfField[3];
        Vignette[] vignette = new Vignette[3];

        transitionVolume.profile.components.Clear();

        foreach (VolumeComponent c in transitionStartProfile.components.AsReadOnly())
        {
            transitionVolume.profile.components.Add(c);
        }

        transitionVolume.profile.TryGet(out bloom[0]); transitionVolume.profile.TryGet(out chromaticAberration[0]); transitionVolume.profile.TryGet(out lensDistortion[0]); transitionVolume.profile.TryGet(out filmGrain[0]); transitionVolume.profile.TryGet(out shadowsMidtonesHighlights[0]); transitionVolume.profile.TryGet(out paniniProjection[0]); transitionVolume.profile.TryGet(out whiteBalance[0]); transitionVolume.profile.TryGet(out depthOfField[0]); transitionVolume.profile.TryGet(out vignette[0]);
        startProfile.TryGet(out bloom[1]); startProfile.TryGet(out chromaticAberration[1]); startProfile.TryGet(out lensDistortion[1]); startProfile.TryGet(out filmGrain[1]); startProfile.TryGet(out shadowsMidtonesHighlights[1]); startProfile.TryGet(out paniniProjection[1]); startProfile.TryGet(out whiteBalance[1]); startProfile.TryGet(out depthOfField[1]); startProfile.TryGet(out vignette[1]);
        endProfile.TryGet(out bloom[2]); endProfile.TryGet(out chromaticAberration[2]); endProfile.TryGet(out lensDistortion[2]); endProfile.TryGet(out filmGrain[2]); endProfile.TryGet(out shadowsMidtonesHighlights[2]); endProfile.TryGet(out paniniProjection[2]); endProfile.TryGet(out whiteBalance[2]); endProfile.TryGet(out depthOfField[2]); endProfile.TryGet(out vignette[2]);



        for (float i = 0; i < Mathf.PI; i += weightTransferIncrement)
        {
            float weight = Mathf.Cos(i + Mathf.PI) / 2 + 0.5f;
            transitionVolume.weight = Blend(0, 1, weight);
            yield return new WaitForEndOfFrame();
        }

        for (float i = 0; i < Mathf.PI; i += increment)
        {
            float weight = Mathf.Cos(i + Mathf.PI) / 2 + 0.5f;
            transitionTransform.position = new Vector3(Blend(startPos.x, finalPos.x, weight), Blend(startPos.y, finalPos.y, weight), Blend(startPos.z, finalPos.z, weight));
            transitionCamera.transform.rotation = Quaternion.Lerp(startCam.transform.rotation, endCam.transform.rotation, i / Mathf.PI);
            transitionCamera.fieldOfView = Blend(startCam.fieldOfView, endCam.fieldOfView, weight);

            // Bloom
            bloom[0].threshold.value = Blend(bloom[1].threshold.value, bloom[2].threshold.value, weight);
            bloom[0].intensity.value = Blend(bloom[1].intensity.value, bloom[2].intensity.value, weight);
            bloom[0].tint.value = Color.Lerp(bloom[1].tint.value, bloom[2].tint.value, weight / Mathf.PI);

            // Chromatic Aberration
            chromaticAberration[0].intensity.value = Blend(chromaticAberration[1].intensity.value, chromaticAberration[2].intensity.value, weight);

            // Lens Distortion
            lensDistortion[0].intensity.value = Blend(lensDistortion[1].intensity.value, lensDistortion[2].intensity.value, weight);

            // Film Grain
            filmGrain[0].intensity.value = Blend(filmGrain[1].intensity.value, filmGrain[2].intensity.value, weight);
            filmGrain[0].response.value = Blend(filmGrain[1].response.value, filmGrain[2].response.value, weight);

            // Shadows Midtones and Highlights
            shadowsMidtonesHighlights[0].shadows.value = Vector4.Lerp(shadowsMidtonesHighlights[1].shadows.value, shadowsMidtonesHighlights[2].shadows.value, weight / Mathf.PI);
            shadowsMidtonesHighlights[0].midtones.value = Vector4.Lerp(shadowsMidtonesHighlights[1].midtones.value, shadowsMidtonesHighlights[2].midtones.value, weight / Mathf.PI);
            shadowsMidtonesHighlights[0].highlights.value = Vector4.Lerp(shadowsMidtonesHighlights[1].highlights.value, shadowsMidtonesHighlights[2].highlights.value, weight / Mathf.PI);
            shadowsMidtonesHighlights[0].shadowsStart.value = Blend(shadowsMidtonesHighlights[1].shadowsStart.value, shadowsMidtonesHighlights[2].shadowsStart.value, weight);
            shadowsMidtonesHighlights[0].shadowsEnd.value = Blend(shadowsMidtonesHighlights[1].shadowsEnd.value, shadowsMidtonesHighlights[2].shadowsEnd.value, weight);

            // Panini Projection
            paniniProjection[0].distance.value = Blend(paniniProjection[1].distance.value, paniniProjection[2].distance.value, weight);
            paniniProjection[0].cropToFit.value = Blend(paniniProjection[1].cropToFit.value, paniniProjection[2].cropToFit.value, weight);

            // White Balance
            whiteBalance[0].temperature.value = Blend(whiteBalance[1].temperature.value, whiteBalance[2].temperature.value, weight);
            whiteBalance[0].tint.value = Blend(whiteBalance[1].tint.value, whiteBalance[2].tint.value, weight);

            // Depth of Field
            depthOfField[0].mode.value = depthOfField[2].mode.value;
            depthOfField[0].gaussianStart.value = Blend(depthOfField[1].gaussianStart.value, depthOfField[2].gaussianStart.value, weight);
            depthOfField[0].gaussianEnd.value = Blend(depthOfField[1].gaussianEnd.value, depthOfField[2].gaussianEnd.value, weight);
            depthOfField[0].focusDistance.value = Blend(depthOfField[1].focusDistance.value, depthOfField[2].focusDistance.value, weight);
            depthOfField[0].focalLength.value = Blend(depthOfField[1].focalLength.value, depthOfField[2].focalLength.value, weight);
            depthOfField[0].aperture.value = Blend(depthOfField[1].aperture.value, depthOfField[2].aperture.value, weight);
            depthOfField[0].bladeCount.value = (int)Blend(depthOfField[1].bladeCount.value, depthOfField[2].bladeCount.value, weight);
            depthOfField[0].bladeCurvature.value = Blend(depthOfField[1].bladeCurvature.value, depthOfField[2].bladeCurvature.value, weight);
            depthOfField[0].bladeRotation.value = Blend(depthOfField[1].bladeRotation.value, depthOfField[2].bladeRotation.value, weight);

            // Vignette
            vignette[0].color.value = Color.Lerp(vignette[1].color.value, vignette[2].color.value, Mathf.PI / weight);
            vignette[0].intensity.value = Blend(vignette[1].intensity.value, vignette[2].intensity.value, weight);
            vignette[0].smoothness.value = Blend(vignette[1].smoothness.value, vignette[2].smoothness.value, weight);

            yield return new WaitForEndOfFrame();
        }

        for (float i = 0; i < Mathf.PI; i += weightTransferIncrement)
        {
            float weight = Mathf.Cos(i + Mathf.PI) / 2 + 0.5f;
            transitionVolume.weight = Blend(1, 0, weight);
            yield return new WaitForEndOfFrame();
        }

        EndTransition();
        yield break;
    }

    private float Blend(float start, float end, float weight)
    {
        return (end * weight) + (start * (1 - weight));
    }
}
