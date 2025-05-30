﻿#define EIJIS_ISSUE_FIX
#define EIJIS_INDEPENDENCE_CUE

using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using static VRC.Core.ApiAvatar;
//
[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
#if EIJIS_INDEPENDENCE_CUE
public class IndependenceCueController : UdonSharpBehaviour
#else
public class CueController : UdonSharpBehaviour
#endif
{
#if !EIJIS_INDEPENDENCE_CUE
    [SerializeField] private BilliardsModule table;
#endif

    [SerializeField] private GameObject primary;
    [SerializeField] private GameObject secondary;
    [SerializeField] private GameObject desktop;
    [SerializeField] private GameObject body;
    [SerializeField] private GameObject cuetip;
#if EIJIS_INDEPENDENCE_CUE
    [SerializeField] private GameObject axisLine;
    [SerializeField] private GameObject bridgePoint;
    const string uniform_marker_colour = "_Color";
    private Color gripColorActive = new Color(0.0f, 0.5f, 1.1f, 1.0f);
    private Color gripColorInactive = new Color(0.34f, 0.34f, 0.34f, 1.0f);
    [SerializeField] private MeshRenderer cuePrimaryGripRenderer;
    [SerializeField] private MeshRenderer cueSecondaryGripRenderer;
    [SerializeField] private GameObject controllerDescriptionPanel;
    private VRCPlayerApi playerLocal;
    private Vector3 controllerDescriptionPanelLocalPos;
    [SerializeField] private GameObject controllerDescriptionPanel_RT;
    [SerializeField] private GameObject controllerDescriptionPanel_RG;
    [SerializeField] private GameObject controllerDescriptionPanel_LT;
    [SerializeField] private GameObject controllerDescriptionPanel_LG;
    [SerializeField] private Material cueSecondaryMaterial;
    [SerializeField] private Material cueSecondaryInvisibleMaterial;
#endif

    [UdonSynced] private byte syncedCueSkin;
    private int activeCueSkin;

    private bool holderIsDesktop;
    [UdonSynced] private bool syncedHolderIsDesktop;

#if EIJIS_INDEPENDENCE_CUE
    [UdonSynced] private bool primaryHolding;
#else
    private bool primaryHolding;
#endif
    [UdonSynced] private bool primaryLocked;
    [UdonSynced] private Vector3 primaryLockPos;
    [UdonSynced] private Vector3 primaryLockDir;

#if EIJIS_INDEPENDENCE_CUE
    [UdonSynced] private bool secondaryHolding;
#else
    private bool secondaryHolding;
#endif
    [UdonSynced] private bool secondaryLocked;
    [UdonSynced] private Vector3 secondaryLockPos;

    private float cueScaleMine = 1;
    [UdonSynced] private float cueScale = 1;
    private float cueSmoothingLocal = 1;
    private float cueSmoothing = 30;

    private Vector3 secondaryOffset;

    private Vector3 origPrimaryPosition;
    private Vector3 origSecondaryPosition;

    private Vector3 lagPrimaryPosition;
    private Vector3 lagSecondaryPosition;

#if EIJIS_INDEPENDENCE_CUE
    private IndependenceCueGrip primaryController;
    private IndependenceCueGrip secondaryController;
#else
    private CueGrip primaryController;
    private CueGrip secondaryController;
#endif

    private Renderer cueRenderer;

    private float gripSize;
    private float cuetipDistance;

#if !EIJIS_INDEPENDENCE_CUE
    private int[] authorizedOwners;
#endif

    [NonSerialized] public bool TeamBlue;

#if EIJIS_INDEPENDENCE_CUE
    private void Start()
    {
        _Init();
        _Enable();
    }
#endif

    public void _Init()
    {
        cueRenderer = this.transform.Find("body/render").GetComponent<Renderer>();

#if EIJIS_INDEPENDENCE_CUE
        primaryController = primary.GetComponent<IndependenceCueGrip>();
        secondaryController = secondary.GetComponent<IndependenceCueGrip>();
#else
        primaryController = primary.GetComponent<CueGrip>();
        secondaryController = secondary.GetComponent<CueGrip>();
#endif
        primaryController._Init(this, false);
        secondaryController._Init(this, true);

        gripSize = 0.03f;
        cuetipDistance = (cuetip.transform.position - primary.transform.position).magnitude;

        origPrimaryPosition = primary.transform.position;
        origSecondaryPosition = secondary.transform.position;

        lagPrimaryPosition = origPrimaryPosition;
        lagSecondaryPosition = origSecondaryPosition;

        resetSecondaryOffset();

#if EIJIS_INDEPENDENCE_CUE
        _EnableRenderer();
#else
        _DisableRenderer();
#endif
#if EIJIS_INDEPENDENCE_CUE
        if (!ReferenceEquals(null, axisLine)) axisLine.SetActive(false);
        if (!ReferenceEquals(null, bridgePoint)) bridgePoint.SetActive(false);
        if (!ReferenceEquals(null, controllerDescriptionPanel))
        {
            playerLocal = Networking.LocalPlayer;
            controllerDescriptionPanelLocalPos = controllerDescriptionPanel.transform.localPosition;
            controllerDescriptionPanel_RT.SetActive(false);
            controllerDescriptionPanel_RG.SetActive(false);
            controllerDescriptionPanel_LT.SetActive(false);
            controllerDescriptionPanel_LG.SetActive(false);
        }
#endif
    }

    public override void OnDeserialization()
    {
#if !EIJIS_INDEPENDENCE_CUE
#if EIJIS_ISSUE_FIX
        VRCPlayerApi ownerPlayer = Networking.GetOwner(this.gameObject);
        if (ReferenceEquals(null, ownerPlayer))
        {
            return;
        }
        int owner = ownerPlayer.playerId;
#else
        int owner = Networking.GetOwner(this.gameObject).playerId;
#endif
        //activeCueSkin = table._CanUseCueSkin(owner, syncedCueSkin) ? syncedCueSkin : 0;
        activeCueSkin = syncedCueSkin;//table._CanUseCueSkin(owner, syncedCueSkin);

        refreshCueSkin();
        refreshCueScale();
#endif
#if EIJIS_INDEPENDENCE_CUE
        if (primaryHolding)
        {
            if (Networking.LocalPlayer.IsOwner(gameObject))
            {
                cuePrimaryGripRenderer.material.SetColor(uniform_marker_colour, gripColorActive);
                cueSecondaryGripRenderer.material.SetColor(uniform_marker_colour, gripColorActive);
            }
            else
            {
                cuePrimaryGripRenderer.material.SetColor(uniform_marker_colour, gripColorInactive);
                cueSecondaryGripRenderer.material.SetColor(uniform_marker_colour, gripColorInactive);
            }
            if (!syncedHolderIsDesktop)
            {
                if (primaryLocked && secondaryHolding)
                {
                    cueSecondaryGripRenderer.material = cueSecondaryInvisibleMaterial;
                }
                else
                {
                    cueSecondaryGripRenderer.material = cueSecondaryMaterial;
                }
                
                if (!secondaryLocked)
                {
                    secondaryController._Show();
                }
            }
        }
        else
        {
            secondaryController._Hide();
        }
        
        if (!ReferenceEquals(null, axisLine))
        {
            axisLine.SetActive(primaryLocked);
        }

        if (!ReferenceEquals(null, bridgePoint))
        {
            if (secondaryLocked && !primaryLocked)
            {
                Quaternion rot = body.transform.rotation;
                bridgePoint.transform.SetPositionAndRotation(secondaryLockPos, rot);
            }
            bridgePoint.SetActive(secondaryLocked && !primaryLocked);
        }

        if (!ReferenceEquals(null, controllerDescriptionPanel))
        {
            controllerDescriptionPanel_RT.SetActive(primaryLocked);
            controllerDescriptionPanel_RG.SetActive(primaryHolding);
            controllerDescriptionPanel_LT.SetActive(secondaryLocked);
            controllerDescriptionPanel_LG.SetActive(secondaryHolding);
        }
#endif
    }

#if !EIJIS_INDEPENDENCE_CUE
    private void refreshCueSkin()
    {
        cueRenderer = this.transform.Find("body/render").GetComponent<Renderer>();
        cueRenderer.materials[1].SetTexture("_MainTex", table.cueSkins[activeCueSkin]);
    }

    private void refreshCueScale()
    {
        body.transform.localScale = new Vector3(1, 1, cueScale);
    }

    private void refreshCueSmoothing()
    {
#if EIJIS_ISSUE_FIX
        if (ReferenceEquals(null, Networking.LocalPlayer)) return;
#endif
        if (!Networking.LocalPlayer.IsOwner(gameObject) || !primaryHolding)
        {
            cueSmoothing = 30;
            return;
        }
        cueSmoothing = 30 * cueSmoothingLocal;
    }

    public void _SetAuthorizedOwners(int[] newOwners)
    {
        authorizedOwners = newOwners;
    }

#endif
    public void _Enable()
    {
        primaryController._Show();
    }

    public void _Disable()
    {
        primaryController._Hide();
        secondaryController._Hide();
    }

    public void _ResetCuePosition()
    {
        if (Networking.LocalPlayer.IsOwner(gameObject))
        {
            resetPosition();
        }
#if EIJIS_INDEPENDENCE_CUE
        else if (!primaryHolding)
        {
            takeOwnership();
            resetPosition();
        }
#endif
    }
#if !EIJIS_INDEPENDENCE_CUE
    public void _RefreshTable()
    {
        Vector3 newpos;
        if (TeamBlue)
        {
            newpos = table.tableModels[table.tableModelLocal].CueBlue.position;
        }
        else
        {
            newpos = table.tableModels[table.tableModelLocal].CueOrange.position;
        }
        primary.transform.localRotation = Quaternion.identity;
        secondary.transform.localRotation = Quaternion.identity;
        desktop.transform.localRotation = Quaternion.identity;
        origPrimaryPosition = newpos;
        primary.transform.position = origPrimaryPosition;
        origSecondaryPosition = primary.transform.TransformPoint(secondaryOffset);
        secondary.transform.position = origSecondaryPosition;
        lagSecondaryPosition = origSecondaryPosition;
        lagPrimaryPosition = origPrimaryPosition;
        desktop.transform.position = origPrimaryPosition;
        body.transform.position = origPrimaryPosition;
    }
    public void UpdateDesktopPosition()
    {
        desktop.transform.position = body.transform.position;
        desktop.transform.rotation = body.transform.rotation;
    }
#endif
#if false // EIJIS_INDEPENDENCE_CUE
    private void Update()
    {
        if (!ReferenceEquals(null, controllerDescriptionPanel))
        {
            var direction = controllerDescriptionPanel.transform.position - Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position;
            if (direction != Vector3.zero)
            {
                var lookRotation = Quaternion.LookRotation(direction, Vector3.up);
                controllerDescriptionPanel.transform.rotation = Quaternion.Lerp(controllerDescriptionPanel.transform.rotation, lookRotation, 0.1f);
            }
            // controllerDescriptionPanel.transform.LookAt(Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position);
        }
    }
#endif
    private void FixedUpdate()
    {
#if EIJIS_INDEPENDENCE_CUE
        if (!ReferenceEquals(null, controllerDescriptionPanel))
        {
            controllerDescriptionPanel.transform.position = body.transform.position + controllerDescriptionPanelLocalPos;
            var direction = controllerDescriptionPanel.transform.position - playerLocal.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position;
            if (direction != Vector3.zero)
            {
                var lookRotation = Quaternion.LookRotation(direction, Vector3.up);
                controllerDescriptionPanel.transform.rotation = Quaternion.Lerp(controllerDescriptionPanel.transform.rotation, lookRotation, 0.1f);
            }
        }

#endif
        if (primaryHolding)
        {
            // must not be shooting, since that takes control of the cue object
#if !EIJIS_INDEPENDENCE_CUE
            if (!table.desktopManager._IsInUI() || !table.desktopManager._IsShooting())
#endif
            {
#if EIJIS_INDEPENDENCE_CUE
                if (!primaryLocked)
#else
                if (!primaryLocked || table.noLockingLocal)
#endif
                {
                    // base of cue goes to primary
                    body.transform.position = lagPrimaryPosition;

                    // holding in primary hand
                    if (!secondaryHolding)
                    {
                        // nothing in secondary hand. have the second grip track the cue
                        secondary.transform.position = primary.transform.TransformPoint(secondaryOffset);
                        body.transform.LookAt(lagSecondaryPosition);
                    }
                    else if (!secondaryLocked)
                    {
                        // holding secondary hand. have cue track the second grip
                        body.transform.LookAt(lagSecondaryPosition);
                    }
                    else
                    {
                        // locking secondary hand. lock rotation on point
                        body.transform.LookAt(secondaryLockPos);
                    }

                    // copy z rotation of primary
                    float rotation = primary.transform.localEulerAngles.z;
                    Vector3 bodyRotation = body.transform.localEulerAngles;
                    bodyRotation.z = rotation;
                    body.transform.localEulerAngles = bodyRotation;
                }
                else
                {
                    // locking primary hand. fix cue in line and ignore secondary hand
                    Vector3 delta = lagPrimaryPosition - primaryLockPos;
                    float distance = Vector3.Dot(delta, primaryLockDir);
                    body.transform.position = primaryLockPos + primaryLockDir * distance;
                }
#if !EIJIS_INDEPENDENCE_CUE

                UpdateDesktopPosition();
#endif
            }
#if !EIJIS_INDEPENDENCE_CUE
            else
            {
                body.transform.position = desktop.transform.position;
                body.transform.rotation = desktop.transform.rotation;
            }

            // clamp controllers
            clampControllers();
#endif
        }
        else
        {
            // other player has cue
            if (!syncedHolderIsDesktop)
            {
                // other player is in vr, use the grips which update faster
#if EIJIS_INDEPENDENCE_CUE
                if (!primaryLocked)
#else
                if (!primaryLocked || table.noLockingLocal)
#endif
                {
                    // base of cue goes to primary
                    body.transform.position = lagPrimaryPosition;

                    // holding in primary hand
                    if (!secondaryLocked)
                    {
                        // have cue track the second grip
                        body.transform.LookAt(lagSecondaryPosition);
                    }
                    else
                    {
                        // locking secondary hand. lock rotation on point
                        body.transform.LookAt(secondaryLockPos);
                    }
                }
                else
                {
                    // locking primary hand. fix cue in line and ignore secondary hand
                    Vector3 delta = lagPrimaryPosition - primaryLockPos;
                    float distance = Vector3.Dot(delta, primaryLockDir);
                    body.transform.position = primaryLockPos + primaryLockDir * distance;
                }
            }
#if !EIJIS_INDEPENDENCE_CUE
            else
            {
                // other player is on desktop, use the slower synced marker
                body.transform.position = desktop.transform.position;
                body.transform.rotation = desktop.transform.rotation;
            }
#endif
        }

        // todo: ugly ugly hack from legacy 8ball. intentionally smooth/lag the position a bit
        // we can't remove this because this directly affects physics
        // must occur at the end after we've finished updating the transform's position
        // otherwise vrchat will try to change it because it's a pickup
        lagPrimaryPosition = Vector3.Lerp(lagPrimaryPosition, primary.transform.position, 1 - Mathf.Pow(0.5f, Time.fixedDeltaTime * cueSmoothing));
        if (!secondaryLocked)
            lagSecondaryPosition = Vector3.Lerp(lagSecondaryPosition, secondary.transform.position, 1 - Mathf.Pow(0.5f, Time.fixedDeltaTime * cueSmoothing));
    }

#if !EIJIS_INDEPENDENCE_CUE
    private Vector3 clamp(Vector3 input, float minX, float maxX, float minY, float maxY, float minZ, float maxZ)
    {
        input.x = Mathf.Clamp(input.x, minX, maxX);
        input.y = Mathf.Clamp(input.y, minY, maxY);
        input.z = Mathf.Clamp(input.z, minZ, maxZ);
        return input;
    }

#endif
    private void resetSecondaryOffset()
    {
        Vector3 position = primary.transform.InverseTransformPoint(secondary.transform.position);
        secondaryOffset = position.normalized * Mathf.Clamp(position.magnitude, gripSize * 2, cuetipDistance);
    }

    private void takeOwnership()
    {
        Networking.SetOwner(Networking.LocalPlayer, this.gameObject);
        Networking.SetOwner(Networking.LocalPlayer, primary);
        Networking.SetOwner(Networking.LocalPlayer, secondary);
        Networking.SetOwner(Networking.LocalPlayer, desktop);
    }

    private void resetPosition()
    {
        primary.transform.position = origPrimaryPosition;
        primary.transform.localRotation = Quaternion.identity;
        secondary.transform.position = origSecondaryPosition;
        secondary.transform.localRotation = Quaternion.identity;
        desktop.transform.position = origPrimaryPosition;
        desktop.transform.localRotation = Quaternion.identity;
        body.transform.position = origPrimaryPosition;
        body.transform.LookAt(origSecondaryPosition);
    }

    public void _OnPrimaryPickup()
    {
        takeOwnership();

#if false // EIJIS_INDEPENDENCE_CUE
        holderIsDesktop = false;
#else
        holderIsDesktop = !Networking.LocalPlayer.IsUserInVR();
#endif
        syncedHolderIsDesktop = holderIsDesktop;
        primaryHolding = true;
        primaryLocked = false;
#if !EIJIS_INDEPENDENCE_CUE
        syncedCueSkin = (byte)table._CanUseCueSkin(Networking.GetOwner(this.gameObject).playerId, syncedCueSkin);
#endif
        //syncedCueSkin = table.activeCueSkin;
        cueScale = cueScaleMine;
        RequestSerialization();
        OnDeserialization();

#if !EIJIS_INDEPENDENCE_CUE
        refreshCueSmoothing();

        table._OnPickupCue();

        if (!holderIsDesktop) secondaryController._Show();
#endif
    }

    public void _OnPrimaryDrop()
    {
        primaryHolding = false;
        syncedHolderIsDesktop = false;
        RequestSerialization();
        OnDeserialization();

#if !EIJIS_INDEPENDENCE_CUE
        refreshCueSmoothing();

#endif
        // hide secondary
        if (!holderIsDesktop) secondaryController._Hide();

#if !EIJIS_INDEPENDENCE_CUE
        // clamp again
        clampControllers();

#endif
        // make sure lag position is reset
        lagPrimaryPosition = primary.transform.position;
        lagSecondaryPosition = secondary.transform.position;

        // move cue to primary grip, since it should be bounded
        body.transform.position = primary.transform.position;
        // make sure cue is facing the secondary grip (since it may have flown off)
        body.transform.LookAt(secondary.transform.position);
        // copy z rotation of primary
        float rotation = primary.transform.localEulerAngles.z;
        Vector3 bodyRotation = body.transform.localEulerAngles;
        bodyRotation.z = rotation;
        body.transform.localEulerAngles = bodyRotation;
        // rotate primary grip to face cue, since cue is visual source of truth
        primary.transform.rotation = body.transform.rotation;
        // reset secondary offset
        resetSecondaryOffset();
#if !EIJIS_INDEPENDENCE_CUE
        // update desktop marker
        UpdateDesktopPosition();

        table._OnDropCue();
#endif
    }

    public void _OnPrimaryUseDown()
    {
        if (!holderIsDesktop)
        {
            primaryLocked = true;
            primaryLockPos = body.transform.position;
            primaryLockDir = body.transform.forward.normalized;
            RequestSerialization();
#if EIJIS_INDEPENDENCE_CUE
            OnDeserialization();
#endif

#if !EIJIS_INDEPENDENCE_CUE
            table._TriggerCueActivate();
#endif
        }
    }

    public void _OnPrimaryUseUp()
    {
        if (!holderIsDesktop)
        {
            primaryLocked = false;
            RequestSerialization();
#if EIJIS_INDEPENDENCE_CUE
            OnDeserialization();
#endif

#if !EIJIS_INDEPENDENCE_CUE
            table._TriggerCueDeactivate();
#endif
        }
    }

    public void _OnSecondaryPickup()
    {
        secondaryHolding = true;
        secondaryLocked = false;
        RequestSerialization();
#if EIJIS_INDEPENDENCE_CUE
        OnDeserialization();
#endif
    }

    public void _OnSecondaryDrop()
    {
        secondaryHolding = false;

        resetSecondaryOffset();
#if EIJIS_INDEPENDENCE_CUE
        OnDeserialization();
#endif
    }

    public void _OnSecondaryUseDown()
    {
        secondaryLocked = true;
        secondaryLockPos = secondary.transform.position;

        RequestSerialization();
#if EIJIS_INDEPENDENCE_CUE
        OnDeserialization();
#endif
    }

    public void _OnSecondaryUseUp()
    {
        secondaryLocked = false;

        RequestSerialization();
#if EIJIS_INDEPENDENCE_CUE
        OnDeserialization();
#endif
    }

#if !EIJIS_INDEPENDENCE_CUE
    public void _RefreshRenderer()
    {
        cueRenderer.enabled = true;
        // enable if live, in LoD range,
        // disable second cue if in practice mode
        if (table.gameLive && !table.localPlayerDistant && (!table.isPracticeMode || this == table.cueControllers[0]))
            cueRenderer.enabled = true;
        else
            cueRenderer.enabled = false;
    }

#endif
    public void _EnableRenderer()
    {
        cueRenderer.enabled = true;
    }

    public void _DisableRenderer()
    {
        cueRenderer.enabled = false;
    }

#if !EIJIS_INDEPENDENCE_CUE
    public void setSmoothing(float smoothing)
    {
        cueSmoothingLocal = smoothing;
        refreshCueSmoothing();
    }

    public void setScale(float scale)
    {
        cueScaleMine = scale;
        if (!Networking.IsOwner(gameObject)) return;
        cueScale = cueScaleMine;
        RequestSerialization();
        OnDeserialization();
    }

    public void resetScale()
    {
        if (!Networking.IsOwner(gameObject)) return;
        if (cueScale == 1) return;
        cueScale = 1;
        RequestSerialization();
        OnDeserialization();
    }

    private void clampControllers()
    {
        clampTransform(primary.transform);
        clampTransform(secondary.transform);
    }

    private void clampTransform(Transform child)
    {
        child.position = table.transform.TransformPoint(clamp(table.transform.InverseTransformPoint(child.position), -4.25f, 4.25f, 0f, 4f, -3.5f, 3.5f));
    }

    public GameObject _GetDesktopMarker()
    {
        return desktop;
    }

    public GameObject _GetCuetip()
    {
        return cuetip;
    }

    public VRCPlayerApi _GetHolder()
    {
        return ((VRC_Pickup)primary.GetComponent(typeof(VRC_Pickup))).currentPlayer;
    }
#endif
}
