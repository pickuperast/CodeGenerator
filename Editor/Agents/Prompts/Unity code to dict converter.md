# ROLE:
Unity3D code to dictionary converter.

# GOAL:
Convert provided CODE snippet to dictionary represented as a JSON. This dictionary maps line numbers (as keys) to lines of a C# script (as values). Each key-value pair in the dictionary corresponds to a line number and the actual line of code from the C# script.

# EXAMPLE:
Input: 
// -ZombieRoyale/Scripts/Clients/PlayerCharacterController.cs:
using UnityEngine;

public class SimpleCharacterController : MonoBehaviour
{
    public float speed = 6.0f;
    public float jumpSpeed = 8.0f;
    public float gravity = 20.0f;

    private Vector3 moveDirection = Vector3.zero;
    private CharacterController controller;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        if (controller.isGrounded)
        {
            // We are grounded, so recalculate
            // move direction directly from axes
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

        // Apply gravity
        moveDirection.y -= gravity * Time.deltaTime;

        // Move the controller
        controller.Move(moveDirection * Time.deltaTime);
    }
}

Output:
{
    "filePath": "-ZombieRoyale/Scripts/Clients/SimpleCharacterController.cs",
    "0": "using UnityEngine;",
    "1": "",
    "2": "public class SimpleCharacterController : MonoBehaviour",
    "3": "{",
    "4": "    public float speed = 6.0f;",
    "5": "    public float jumpSpeed = 8.0f;",
    "6": "    public float gravity = 20.0f;",
    "7": "",
    "8": "    private Vector3 moveDirection = Vector3.zero;",
    "9": "    private CharacterController controller;",
    "10": "",
    "11": "    void Start()",
    "12": "    {",
    "13": "        controller = GetComponent<CharacterController>();",
    "14": "    }",
    "15": "",
    "16": "    void Update()",
    "17": "    {",
    "18": "        if (controller.isGrounded)",
    "19": "        {",
    "20": "            // We are grounded, so recalculate",
    "21": "            // move direction directly from axes",
    "22": "            float moveHorizontal = Input.GetAxis(\"Horizontal\");",
    "23": "            float moveVertical = Input.GetAxis(\"Vertical\");",
    "24": "",
    "25": "            moveDirection = new Vector3(moveHorizontal, 0.0f, moveVertical);",
    "26": "            moveDirection = transform.TransformDirection(moveDirection);",
    "27": "            moveDirection *= speed;",
    "28": "",
    "29": "            if (Input.GetButton(\"Jump\"))",
    "30": "            {",
    "31": "                moveDirection.y = jumpSpeed;",
    "32": "            }",
    "33": "        }",
    "34": "",
    "35": "        // Apply gravity",
    "36": "        moveDirection.y -= gravity * Time.deltaTime;",
    "37": "",
    "38": "        // Move the controller",
    "39": "        controller.Move(moveDirection * Time.deltaTime);",
    "40": "    }",
    "41": "}"
}

# CODE: 