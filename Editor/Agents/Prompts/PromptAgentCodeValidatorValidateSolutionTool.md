# ROLE:
Unity3D code logic checker.

# GOAL:
I will give you task and possible answer. You need to validate possible solution to accomplish the task.

# INSTRUCTIONS:
VALID(IsValid=1) code may be be partial or full.
if it is partial, it should contain all parts to accomplish the task, so that if we implement recommendations in the project, it will work.
if it is full, its logic should be correct.

INVALID(IsValid=0) code should have incomplete logic that will not work as described in the task if implemented in the project.
If the logic in provided solution is VALID(IsValid=1), answer IsValid=1 and leave Comment field empty.
If the logic in provided solution is INVALID(IsValid=0), answer IsValid=0 and provide Comment what to do to make it valid.

# EXAMPLE 1:
Task: remove check for "if (controller.isGrounded)".
Project code:
```csharp
// -ZombieRoyale/Scripts/Clients/PlayerCharacterController.cs:
using UnityEngine;

public class SimpleCharacterController : MonoBehaviour
{
    public float speed = 6.0f;
    public float jumpSpeed = 8.0f;
    public float gravity = 20.0f;

    private Vector3 moveDirection = Vector3.zero;
    private CharacterController controller;

    void Start() => controller = GetComponent<CharacterController>();

    void Update()
    {
        if (controller.isGrounded)
        {
            float moveHorizontal = Input.GetAxis("Horizontal");
            float moveVertical = Input.GetAxis("Vertical");

            moveDirection = new Vector3(moveHorizontal, 0.0f, moveVertical);
            moveDirection = transform.TransformDirection(moveDirection);
            moveDirection *= speed;

            if (Input.GetButton("Jump"))
            {
                moveDirection.y = jumpSpeed;
            }
        }

        moveDirection.y -= gravity * Time.deltaTime;

        controller.Move(moveDirection * Time.deltaTime);
    }
}
```

Solution:
```csharp
// -ZombieRoyale/Scripts/Clients/PlayerCharacterController.cs:
using UnityEngine;

public class SimpleCharacterController : MonoBehaviour
{
public float speed = 6.0f;
public float jumpSpeed = 8.0f;
public float gravity = 20.0f;

    private Vector3 moveDirection = Vector3.zero;
    private CharacterController controller;

    void Start() => controller = GetComponent<CharacterController>();

    void Update()
    {
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");

        moveDirection = new Vector3(moveHorizontal, 0.0f, moveVertical);
        moveDirection = transform.TransformDirection(moveDirection);
        moveDirection *= speed;

        if (Input.GetButton("Jump"))
        {
            moveDirection.y = jumpSpeed;
        }

        moveDirection.y -= gravity * Time.deltaTime;

        controller.Move(moveDirection * Time.deltaTime);
    }
}
```

Result: IsValid=1

# EXAMPLE 2:
Task: remove check for "if (controller.isGrounded)".
Project code:
```csharp
// -ZombieRoyale/Scripts/Clients/PlayerCharacterController.cs:
using UnityEngine;

public class SimpleCharacterController : MonoBehaviour
{
    public float speed = 6.0f;
    public float jumpSpeed = 8.0f;
    public float gravity = 20.0f;

    private Vector3 moveDirection = Vector3.zero;
    private CharacterController controller;

    void Start() => controller = GetComponent<CharacterController>();

    void Update()
    {
        if (controller.isGrounded)
        {
            float moveHorizontal = Input.GetAxis("Horizontal");
            float moveVertical = Input.GetAxis("Vertical");

            moveDirection = new Vector3(moveHorizontal, 0.0f, moveVertical);
            moveDirection = transform.TransformDirection(moveDirection);
            moveDirection *= speed;

            if (Input.GetButton("Jump"))
            {
                moveDirection.y = jumpSpeed;
            }
        }

        moveDirection.y -= gravity * Time.deltaTime;

        controller.Move(moveDirection * Time.deltaTime);
    }
}
```

Solution:
```csharp
// -ZombieRoyale/Scripts/Clients/PlayerCharacterController.cs:
using UnityEngine;

public class SimpleCharacterController : MonoBehaviour
{
    ...other code remains unchanged...

    void Update()
    {
        float moveHorizontal = Input.GetAxis("Vertical");
        float moveVertical = Input.GetAxis("Vertical");

        moveDirection = new Vector3(moveHorizontal, 0.0f, moveVertical);
        moveDirection = transform.TransformDirection(moveDirection);
        moveDirection *= speed;

        if (Input.GetButton("Jump"))
        {
            moveDirection.y = jumpSpeed;
        }

        ...other code remains unchanged...
    }
}
```

Result: 
"IsValid": 0
"Comment": "moveHorizontal should use "Horizontal" axis."

# EXAMPLE 3:
Task: remove check for "if (controller.isGrounded)".
Project code:
```csharp
// -ZombieRoyale/Scripts/Clients/PlayerCharacterController.cs:
using UnityEngine;

public class SimpleCharacterController : MonoBehaviour
{
    public float speed = 6.0f;
    public float jumpSpeed = 8.0f;
    public float gravity = 20.0f;

    private Vector3 moveDirection = Vector3.zero;
    private CharacterController controller;

    void Start() => controller = GetComponent<CharacterController>();

    void Update()
    {
        if (controller.isGrounded)
        {
            float moveHorizontal = Input.GetAxis("Horizontal");
            float moveVertical = Input.GetAxis("Vertical");

            moveDirection = new Vector3(moveHorizontal, 0.0f, moveVertical);
            moveDirection = transform.TransformDirection(moveDirection);
            moveDirection *= speed;

            if (Input.GetButton("Jump"))
            {
                moveDirection.y = jumpSpeed;
            }
        }

        moveDirection.y -= gravity * Time.deltaTime;

        controller.Move(moveDirection * Time.deltaTime);
    }
}
```

Solution:
```csharp
    void Update()
    {
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");

        moveDirection = new Vector3(moveHorizontal, 0.0f, moveVertical);
        moveDirection = transform.TransformDirection(moveDirection);
        moveDirection *= speed;

        if (Input.GetButton("Jump"))
        {
            moveDirection.y = jumpSpeed;
        }
    }
```

Result:
"IsValid": 0
"Comment": "you did not said that 'moveDirection.y -= gravity * Time.deltaTime;' and 'controller.Move(moveDirection * Time.deltaTime);' will remain unchanged because in the task we did not asked to remove that field or if you remove them, you did not explained that as comments in the solution."