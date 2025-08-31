//==========================================( Neverway 2025 )=========================================================//
// Author
//  Liz M.
// 
// Contributors: 
//  Connorses, Errynei, Soulex
//
//====================================================================================================================//

using RivenFramework;
using UnityEngine;

public class TDPawn_Player : TDPawn
{
    #region========================================( Variables )======================================================//
    /*-----[ Inspector Variables ]------------------------------------------------------------------------------------*/
    /*----------------------------------------------------------------------------------------------------------------*/
    
    
    /*-----[ External Variables ]-------------------------------------------------------------------------------------*/
    /*----------------------------------------------------------------------------------------------------------------*/

    
    /*-----[ Internal Variables ]-------------------------------------------------------------------------------------*/
    /*----------------------------------------------------------------------------------------------------------------*/
    private Vector3 moveDirection;
    private Vector2 lookRotation;
    
    /*-----[ Reference Variables ]------------------------------------------------------------------------------------*/
    /*----------------------------------------------------------------------------------------------------------------*/
    private GI_WidgetManager widgetManager;
    private new TDPawnActions action = new TDPawnActions();
    private InputActions.FirstPersonActions inputActions;
    [SerializeField] private GameObject DeathScreenWidget;
    
    #endregion


    #region=======================================( Functions )=======================================================//
    /*-----[ Mono Functions ]-----------------------------------------------------------------------------------------*/
    /*----------------------------------------------------------------------------------------------------------------*/
    private void UpdatePauseMenu()
    {
        if (!widgetManager)
        {
            widgetManager = FindObjectOfType<GI_WidgetManager>();
            if (!widgetManager) return;
        }
        isPaused = widgetManager.GetExistingWidget("WB_Pause");
        
        // Pause Game
        if (inputActions.Pause.WasPressedThisFrame())
        {
            widgetManager.ToggleWidget("WB_Pause");
        }
        
        // Lock mouse when unpaused, unlock when paused
        if (isPaused)
        {
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }

            return;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    
    public new void Awake()
    {
        base.Awake();
        
        // Subscribe to events
        OnPawnDeath += () => { OnDeath(); };
        
        // Setup inputs
        inputActions = new InputActions().FirstPerson;
        inputActions.Enable();
        
        // Enable the view camera
        action.EnableViewCamera(this, true);
        
        // Disable the mouse cursor
        
    }

    public void Update()
    {
        // Pausing
        UpdatePauseMenu();
        
        
        if (isPaused || isDead) return;
        UpdateMovement();
        
        // Kill bind
        if (Input.GetKeyDown(KeyCode.Delete)) Kill();
        
        // Interact 
        if (inputActions.Interact.WasPressedThisFrame()) action.Interact(this, interactionPrefab, viewPoint.transform);
        
        // Throw held object
        //if (inputActions.ItemAction1.WasPressedThisFrame() && physObjectAttachmentPoint.attachedObject)
        //{
        //    action.ThrowPhysProp(this);
        //}
        
        // Switch item
        if (inputActions.ItemSwapNext.WasPressedThisFrame()) action.ItemSwapNext(this);
        if (inputActions.ItemSwapPrevious.WasPressedThisFrame()) action.ItemSwapPrevious(this);
        
        // Use Item
        var inventory = GetComponentInChildren<Pawn_Inventory>();
        if (inputActions.ItemAction1.WasPressedThisFrame()) action.ItemUseAction(inventory, 0);
        if (inputActions.ItemAction2.WasPressedThisFrame()) action.ItemUseAction(inventory, 1);
        if (inputActions.ItemAction3.WasPressedThisFrame()) action.ItemUseAction(inventory, 2);
        if (inputActions.ItemAction1.WasReleasedThisFrame()) action.ItemUseAction(inventory, 0, "release");
        if (inputActions.ItemAction2.WasReleasedThisFrame()) action.ItemUseAction(inventory, 1, "release");
        if (inputActions.ItemAction3.WasReleasedThisFrame()) action.ItemUseAction(inventory, 2, "release");
    }

    public void FixedUpdate()
    {
        if (isPaused || isDead) return;
        ApplyMovement();
    }

    /*-----[ Internal Functions ]-------------------------------------------------------------------------------------*/
    /*----------------------------------------------------------------------------------------------------------------*/

    
    private void UpdateMovement()
    {
        moveDirection = new Vector3(inputActions.Move.ReadValue<Vector2>().x, inputActions.Move.ReadValue<Vector2>().y, 0);
    }
    private void ApplyMovement()
    {
        action.Move(this, moveDirection);
    }


    private void OnDeath()
    {
        // Drop held props
        if (physObjectAttachmentPoint)
        {
            if (physObjectAttachmentPoint.attachedObject)
            {
                if (physObjectAttachmentPoint.attachedObject.GetComponent<Object_PhysPickup>())
                {
                    physObjectAttachmentPoint.attachedObject.GetComponent<Object_PhysPickup>().ToggleHeld();
                }
            }
        }

        // Remove the HUD
        Destroy(widgetManager.GetExistingWidget("WB_HUD"));
        // Add the respawn HUD
        widgetManager.AddWidget(DeathScreenWidget);

        // Play the death animation
        if (GetComponent<Animator>()) GetComponent<Animator>().Play("Death");
    }

    /*-----[ External Functions ]-------------------------------------------------------------------------------------*/
    /*----------------------------------------------------------------------------------------------------------------*/


    #endregion
}
