using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct ROD
{
    public int player;
    public int rod_player_count;
    public float rod_player_spacing;
    public string rod_name;
    

    public ROD(string rod_name, int player, int rod_player_count, float rod_player_spacing)
    {
        this.rod_name = rod_name;
        this.player = player;
        this.rod_player_count = rod_player_count;
        this.rod_player_spacing = rod_player_spacing;
    }
}

public enum PHYSICS_LAYERS
{
    Table = 8,
    Rods = 9,
    RodBumpers = 10,
    Ball = 11,
    BallDetector = 12
}



public class CreateRandomTable : MonoBehaviour {
    public List<Material> materials_exterior;
    public List<Material> materials_play_surface;
    public List<Material> materials_inside_wall;
    public List<Material> materials_ball;
    public List<Material> materials_rod;
    public List<Material> materials_bumper;
    public List<Material> materials_player1;
    public List<Material> materials_player2;
    public List<Material> materials_handle1;
    public List<Material> materials_handle2;

    // Use this for initialization
    void Awake () {
        // Estimated masses in grams
        float rod_mass = 700f * Random.Range(0.8f, 1.2f);
        float player_mass = 70f * Random.Range(0.8f, 1.2f);
        float ball_mass = 25f * Random.Range(0.8f, 1.2f);

        // Tornado T-3000 Measurements from Dan Packer
        float ball_diameter = 0.32f; // (cm). Ball diameter
        float bumper_diameter = 0.28f; // (cm). Diameter of rod bumper
        float bumper_length = 0.25f; // (cm). Length of rod bumper
        float goal_height = 0.8f; // (cm). Goal net height
        float goal_width = 2.04f; // (cm). Goal net width
        float player_chest_width = 0.32f; // (cm). Player width from the side view, the diameter of the chest part that the rod goes through
        float player_toes_to_surface = 0.05f; // (cm). When a player is perfectly vertical, how big is the gap from the player foot to the table
        float player_toes_width = 0.22f; // (cm). Foot width of the player feet (along the axis of the rod)
        float player_goalie_toes_to_wall = 0.03f; // (cm). 
        float player_head_to_toes = 1.08f; // (cm). Player height from top of head to bottom of foot
        float player_rod_to_head = 0.35f; // (cm). 
        float player_rod_to_toes = 0.73f; // (cm). 
        float player_width = 0.3f; // (cm). Player width of a player mid-center along axis of the rod. Basically measure the length of the hole in the player.
        float rod_2bar_player_spacing = 2.1f; // (cm). Spacing between players. Measure from inside edge of player to inside edge of player.
        float rod_3bar_player_spacing = 1.52f; // (cm). Spacing between players. Measure from inside edge of player to inside edge of player.
        float rod_5bar_player_spacing = 0.9f; // (cm). Spacing between players. Measure from inside edge of player to inside edge of player.
        float rod_bearing_width = 0.44f; // (cm). Width of a bearing installed on the table, from flat part of hex to flat, down the middle
        float rod_diameter = 0.15f; // (cm). Rod diameter
        float rod_goalie_player_spacing = 1.75f; // (cm). Spacing between players. Measure from inside edge of player to inside edge of player.
        float rod_handle_diameter = 0.34f; // (cm). What is the diameter of a rod handle (just take the large part of the handle)
        float rod_handle_length = 1.24f; // (cm). What is the length of a rod handle?
        float rod_handle_to_bearing = 0.3f; // (cm). When a rod is fully-pushed in, what is the length the edge of the bearing to the rod handle?
        float rod_handle_to_table = 0.32f; // (cm). When a rod is fully-pushed in, what is the length the edge of the table to the rod handle?
        float table_box_height = 4.16f; // (cm). Height from bottom of table to top of table (height without legs)
        float table_full_height = 8.4f; // (cm). Height of table (without screw feet)
        float table_inner_length = 12.76f; // (cm). Inner table length (play area)
        float table_inner_wall_height = 1.12f; // (cm). Inner wall height
        float table_inner_width = 6.82f; // (cm). Inner table width (play area)
        float table_leg_width = 1f; // (cm). Table leg width
        float table_outer_length = 14.2f; // (cm). Outer table length (whole table)
        float table_outer_width = 7.62f; // (cm). Outer table width (including walls)



        // Calculated fields
        float rod_surface_to_rod = player_rod_to_toes + player_toes_to_surface;
        float goal_depth = (table_outer_length - table_inner_length - 0.5f) / 2.0f;
        Material material_exterior = materials_exterior[0];
        Material material_play_surface = materials_play_surface[0];
        Material material_inside_wall = materials_inside_wall[0];
        Material material_ball = materials_ball[0];
        Material material_rod = materials_rod[0];
        Material material_bumper = materials_bumper[0];
        List<Material> material_handle = new List<Material>() { materials_handle1[0], materials_handle2[0] };
        List<Material> material_player = new List<Material>() { materials_player1[0], materials_player2[0] };

        // Physics materials
        PhysicMaterial material_foot_physics = new PhysicMaterial("FootFriction");
        material_foot_physics.bounciness = 0.8f + Random.Range(-0.2f, 0.2f); // loses 20% of energy on bounce
        material_foot_physics.dynamicFriction = 0.2f + Random.Range(-0.2f, 0.2f); // When moving
        material_foot_physics.staticFriction = 0.3f + Random.Range(-0.2f, 0.2f); // When static

        PhysicMaterial material_surface_physics = new PhysicMaterial("SurfaceFriction");
        material_surface_physics.bounciness = 0.95f + Random.Range(-0.2f, 0.05f); // loses 5% of energy on bounce
        material_surface_physics.dynamicFriction = 0.10f + Random.Range(-0.05f, 0.1f); // When moving
        material_surface_physics.staticFriction = 0.10f + Random.Range(-0.05f, 0.1f); // When static

        PhysicMaterial material_wall_physics = new PhysicMaterial("WallFriction");
        material_wall_physics.bounciness = 0.95f + Random.Range(-0.2f, 0.05f); // loses 5% of energy on bounce
        material_wall_physics.dynamicFriction = 0.10f + Random.Range(-0.05f, 0.1f); // When moving
        material_wall_physics.staticFriction = 0.10f + Random.Range(-0.05f, 0.1f); // When static

        // Build the base foosball table base game object
        GameObject go;
        Mesh m;

        GameObject foosball_setup = this.gameObject;

        GameObject table = new GameObject
        {
            name = "Foosball Table"
        };
        table.transform.parent = foosball_setup.transform;
        table.layer = (int)PHYSICS_LAYERS.Table;
        Rigidbody rb = table.AddComponent<Rigidbody>();
        rb.isKinematic = true; // Can't effect the table
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezeAll;
        


        // Base directions:
        //  x -> direction of play
        //  y -> vertical
        //  z -> direction of rods
        float delta = 0.01f;

        // Base
        go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = "BaseExterior";
        go.layer = (int)PHYSICS_LAYERS.Table;
        go.GetComponent<Renderer>().material = material_exterior;
        go.transform.localScale = new Vector3(table_outer_length, table_box_height - table_inner_wall_height, table_outer_width);
        go.transform.position = new Vector3(0, -(table_box_height - table_inner_wall_height)/2.0f, 0);
        go.transform.transform.parent = table.transform;

        go = GameObject.CreatePrimitive(PrimitiveType.Plane);
        go.name = "PlaySurface";
        go.layer = (int) PHYSICS_LAYERS.Table;
        go.GetComponent<Renderer>().material = material_play_surface;
        go.transform.localScale = new Vector3(table_inner_length, 1, table_inner_width)/10.0f;
        go.transform.position = new Vector3(0, delta, 0);
        go.transform.transform.parent = table.transform;
        go.GetComponent<Collider>().material = material_surface_physics;

        // Add the in-play-area detector, to detect if a ball has dissapeared for somehow
        go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = "Detector_BallOnTable";
        go.layer = (int)PHYSICS_LAYERS.BallDetector;
        go.GetComponent<BoxCollider>().isTrigger = true;
        go.GetComponent<MeshRenderer>().enabled = false;  // No need to render this
        go.transform.localScale = new Vector3(table_outer_length, table_inner_wall_height+ball_diameter, table_outer_width);
        go.transform.position = new Vector3(0, (table_inner_wall_height + ball_diameter)/2.0f, 0);
        go.transform.transform.parent = table.transform;
        
        // Add the two sides
        go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = "Side1Exterior";
        Destroy(go.GetComponent<BoxCollider>());
        go.GetComponent<Renderer>().material = material_exterior;
        go.transform.localScale = new Vector3(table_outer_length, table_inner_wall_height, (table_outer_width - table_inner_width)/2.0f);
        go.transform.position = new Vector3(0, table_inner_wall_height/2.0f, -(table_outer_width + table_inner_width)/4.0f );
        go.transform.transform.parent = table.transform;

        go = GameObject.CreatePrimitive(PrimitiveType.Plane);
        go.name = "Side1Int";
        go.layer = (int)PHYSICS_LAYERS.Table;
        go.GetComponent<Renderer>().material = material_inside_wall;
        go.transform.localScale = new Vector3(table_inner_length, 1, table_inner_wall_height) / 10.0f;
        go.transform.Rotate(new Vector3(1f, 0, 0), 90.0f);
        go.transform.position = new Vector3(0, table_inner_wall_height / 2.0f, -table_inner_width / 2.0f + delta);
        go.transform.transform.parent = table.transform;
        go.GetComponent<Collider>().material = material_wall_physics;

        go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = "Side2Exterior";
        Destroy(go.GetComponent<BoxCollider>());
        go.GetComponent<Renderer>().material = material_exterior;
        go.transform.localScale = new Vector3(table_outer_length, table_inner_wall_height, (table_outer_width - table_inner_width) / 2.0f);
        go.transform.position = new Vector3(0, table_inner_wall_height / 2.0f, +(table_outer_width + table_inner_width) / 4.0f);
        go.transform.transform.parent = table.transform;

        
        go = GameObject.CreatePrimitive(PrimitiveType.Plane);
        go.name = "Side2Int";
        go.layer = (int)PHYSICS_LAYERS.Table;
        go.GetComponent<Renderer>().material = material_inside_wall;
        go.transform.localScale = new Vector3(table_inner_length, 1, table_inner_wall_height) / 10.0f;
        go.transform.Rotate(new Vector3(1f, 0, 0), -90.0f);
        go.transform.position = new Vector3(0, table_inner_wall_height / 2.0f, table_inner_width / 2.0f - delta);
        go.transform.transform.parent = table.transform;
        go.GetComponent<Collider>().material = material_wall_physics;

        // Build the ball out of play detector box
        go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = "Detector_OutOfPlay";
        go.layer = (int)PHYSICS_LAYERS.BallDetector;
        //go.GetComponent<BoxCollider>().isTrigger = true; // Now it will only detect the ball. Solid for now to bounce ball back in play.
        go.GetComponent<MeshRenderer>().enabled = false;  // No need to render this
        go.transform.localScale = new Vector3(table_outer_length, ball_diameter, table_outer_width);
        go.transform.position = new Vector3(0.0f, 1.49f*ball_diameter + table_inner_wall_height, 0.0f);
        go.transform.transform.parent = table.transform;
        
        // Add the two goal sides as a mesh
        // Base directions:
        //  x -> direction of play
        //  y -> vertical
        //  z -> direction of rods
        List<Vector3> vertices = new List<Vector3>
        {
                // Left of net square
                new Vector3(0f,0f,-table_inner_width/2),
                new Vector3(0f,table_inner_wall_height,-table_inner_width/2),
                new Vector3(0f,table_inner_wall_height,-goal_width/2),
                new Vector3(0f,0f,-goal_width/2),

                // Above net square
                new Vector3(0f,goal_height,-goal_width/2),
                new Vector3(0f,table_inner_wall_height,-goal_width/2),
                new Vector3(0f,table_inner_wall_height,+goal_width/2),
                new Vector3(0f,goal_height,+goal_width/2),

                // Right of net square
                new Vector3(0f,0f,+table_inner_width/2),
                new Vector3(0f,table_inner_wall_height,+table_inner_width/2),
                new Vector3(0f,table_inner_wall_height,+goal_width/2),
                new Vector3(0f,0f,+goal_width/2),
        };
        List<int> triangles = GetTrianglesFromSquares(vertices);
        List<Vector3> normals = GetNormalsFromSquares(vertices);

        go = new GameObject("goal1", typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider));
        m = new Mesh();
        go.layer = (int)PHYSICS_LAYERS.Table;
        go.GetComponent<MeshCollider>().sharedMesh = m;
        go.GetComponent<MeshCollider>().material = material_wall_physics;
        go.GetComponent<MeshFilter>().mesh = m;
        m.vertices = vertices.ToArray();
        m.triangles = triangles.ToArray();
        m.normals = normals.ToArray();
        go.GetComponent<Renderer>().material = material_inside_wall;
        go.transform.position = new Vector3(table_inner_length / 2.0f, 0f, 0f);
        go.transform.transform.parent = table.transform;


        go = new GameObject("goal2", typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider));
        m = new Mesh();
        go.layer = (int)PHYSICS_LAYERS.Table;
        go.GetComponent<MeshCollider>().sharedMesh = m;
        go.GetComponent<MeshCollider>().material = material_wall_physics;
        go.GetComponent<MeshFilter>().mesh = m;
        m.vertices = vertices.ToArray();
        m.triangles = triangles.ToArray();
        go.GetComponent<Renderer>().material = material_inside_wall;
        go.transform.position = new Vector3(-table_inner_length / 2.0f, 0f, 0f);
        go.transform.transform.parent = table.transform;

        // Build the goal detectors boxes
        // Note: We don't want to detect balls that hit the corner, so we start the detection box 1/2 ball width into the goal.
        go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = "Detector_Goal_Player1";
        go.layer = (int)PHYSICS_LAYERS.BallDetector;
        go.GetComponent<BoxCollider>().isTrigger = true; // Now it will only detect the ball
        go.GetComponent<MeshRenderer>().enabled = false;  // No need to render this
        go.transform.localScale = new Vector3((table_outer_length-table_inner_length)/2.0f - ball_diameter/2.0f, table_inner_wall_height, table_inner_width);
        go.transform.position = new Vector3(((table_inner_length+table_outer_length)/4.0f + ball_diameter/4.0f), table_inner_wall_height/2.0f, 0.0f);
        GoalTracking gt = go.AddComponent<GoalTracking>();
        gt.player = 0;
        gt.tableManager = this.GetComponent<TableManager>();
        go.transform.transform.parent = table.transform;
        this.GetComponent<TableManager>().goal1 = go;

        go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = "Detector_Goal_Player2";
        go.layer = (int)PHYSICS_LAYERS.BallDetector;
        go.GetComponent<BoxCollider>().isTrigger = true; // Now it will only detect the ball
        go.GetComponent<MeshRenderer>().enabled = false;  // No need to render this
        go.transform.localScale = new Vector3((table_outer_length - table_inner_length) / 2.0f - ball_diameter / 2.0f, table_inner_wall_height, table_inner_width);
        go.transform.position = new Vector3(-((table_inner_length + table_outer_length) / 4.0f + ball_diameter / 4.0f), table_inner_wall_height / 2.0f, 0.0f);
        gt = go.AddComponent<GoalTracking>();
        gt.player = 1;
        gt.tableManager = this.GetComponent<TableManager>();
        go.transform.transform.parent = table.transform;
        this.GetComponent<TableManager>().goal2 = go;


        // Add the end two sides of the table
        // Base directions:
        //  x -> direction of play
        //  y -> vertical
        //  z -> direction of rods
        vertices = new List<Vector3>
        {
                // Back of table
                new Vector3((table_outer_length-table_inner_length)/2.0f,0f,-table_outer_width/2.0f),
                new Vector3((table_outer_length-table_inner_length)/2.0f,table_box_height,-table_outer_width/2.0f),
                new Vector3((table_outer_length-table_inner_length)/2.0f,table_box_height,+table_outer_width/2.0f),
                new Vector3((table_outer_length-table_inner_length)/2.0f,0f,+table_outer_width/2.0f),

                // Top of table
                new Vector3(0f,table_box_height,-table_outer_width/2.0f),
                new Vector3(0f,table_box_height,+table_outer_width/2.0f),
                new Vector3((table_outer_length-table_inner_length)/2.0f,table_box_height,+table_outer_width/2.0f),
                new Vector3((table_outer_length-table_inner_length)/2.0f,table_box_height,-table_outer_width/2.0f),
        };
        triangles = GetTrianglesFromSquares(vertices);
        normals = GetNormalsFromSquares(vertices);

        go = new GameObject("topgoal1", typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider));
        m = new Mesh();
        go.GetComponent<MeshCollider>().sharedMesh = m;
        go.GetComponent<MeshFilter>().mesh = m;
        go.layer = (int)PHYSICS_LAYERS.Table;
        m.vertices = vertices.ToArray();
        m.triangles = triangles.ToArray();
        m.normals = normals.ToArray();
        go.GetComponent<Renderer>().material = material_exterior;
        go.transform.position = new Vector3(table_inner_length / 2.0f, -(table_box_height - table_inner_wall_height), 0f);
        go.transform.transform.parent = table.transform;

        go = new GameObject("topgoal2", typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider));
        m = new Mesh();
        go.GetComponent<MeshCollider>().sharedMesh = m;
        go.GetComponent<MeshFilter>().mesh = m;
        go.layer = (int)PHYSICS_LAYERS.Table;
        m.vertices = vertices.ToArray();
        m.triangles = triangles.ToArray();
        m.normals = normals.ToArray();
        go.GetComponent<Renderer>().material = material_exterior;
        go.transform.Rotate(new Vector3(0f, 1f, 0f), 180.0f);
        go.transform.position = new Vector3(-table_inner_length / 2.0f, -(table_box_height - table_inner_wall_height), 0f);
        go.transform.transform.parent = table.transform;


        // Add the legs of the table
        // Base directions:
        //  x -> direction of play
        //  y -> vertical
        //  z -> direction of rods
        go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = "Leg1";
        Destroy(go.GetComponent<BoxCollider>());
        go.GetComponent<Renderer>().material = material_exterior;
        go.transform.localScale = new Vector3(table_leg_width, table_full_height - table_box_height, table_leg_width);
        go.transform.position = new Vector3(-(table_outer_length/2.0f - table_leg_width/2), -(table_full_height - table_box_height) / 2.0f - (table_box_height-table_inner_wall_height), -(table_outer_width / 2.0f - table_leg_width / 2));
        go.transform.transform.parent = table.transform;

        go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = "Leg2";
        Destroy(go.GetComponent<BoxCollider>());
        go.GetComponent<Renderer>().material = material_exterior;
        go.transform.localScale = new Vector3(table_leg_width, table_full_height - table_box_height, table_leg_width);
        go.transform.position = new Vector3(-(table_outer_length / 2.0f - table_leg_width / 2), -(table_full_height - table_box_height) / 2.0f - (table_box_height - table_inner_wall_height), (table_outer_width / 2.0f - table_leg_width / 2));
        go.transform.transform.parent = table.transform;

        go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = "Leg3";
        Destroy(go.GetComponent<BoxCollider>());
        go.GetComponent<Renderer>().material = material_exterior;
        go.transform.localScale = new Vector3(table_leg_width, table_full_height - table_box_height, table_leg_width);
        go.transform.position = new Vector3((table_outer_length / 2.0f - table_leg_width / 2), -(table_full_height - table_box_height) / 2.0f - (table_box_height - table_inner_wall_height), -(table_outer_width / 2.0f - table_leg_width / 2));
        go.transform.transform.parent = table.transform;

        go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = "Leg4";
        Destroy(go.GetComponent<BoxCollider>());
        go.GetComponent<Renderer>().material = material_exterior;
        go.transform.localScale = new Vector3(table_leg_width, table_full_height - table_box_height, table_leg_width);
        go.transform.position = new Vector3((table_outer_length / 2.0f - table_leg_width / 2), -(table_full_height - table_box_height) / 2.0f - (table_box_height - table_inner_wall_height), (table_outer_width / 2.0f - table_leg_width / 2));
        go.transform.transform.parent = table.transform;


        // Place the ball
        go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.name = "Ball";
        go.tag = "Ball";
        rb = go.AddComponent<Rigidbody>();
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic; // So it won't fly through other objects if travelling too fast
        rb.mass = ball_mass;
        rb.maxAngularVelocity = 2f * 3.14f * 1000f;
        go.layer = (int)PHYSICS_LAYERS.Ball;
        go.GetComponent<Renderer>().material = material_ball;
        go.transform.localScale = new Vector3(ball_diameter, ball_diameter, ball_diameter);
        go.transform.position = new Vector3(0f, ball_diameter*2.0f, 0f);
        go.transform.transform.parent = table.transform;
        this.GetComponent<TableManager>().ball = go;

        // Place the ball drop source
        go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.name = "BallDropSource";
        go.layer = (int)PHYSICS_LAYERS.BallDetector;
        go.GetComponent<MeshRenderer>().enabled = false;  // No need to render this
        go.transform.localScale = new Vector3(ball_diameter, ball_diameter, ball_diameter);
        go.transform.position = new Vector3(0f, ball_diameter * 3.0f, 0f);
        go.transform.transform.parent = table.transform;
        this.GetComponent<TableManager>().ball_drop_source = go;

        // Place the two agent cameras
        go = new GameObject("Camera Player 1");
        go.AddComponent<Camera>();
        go.transform.position = new Vector3(0f, table_outer_length / 2.0f, 0f) +
                                new Vector3(-table_outer_length * 1.5f / 2.0f, 0f, 0f) +
                                table_outer_length * 0.1f * Random.insideUnitSphere;
        go.transform.transform.parent = table.transform;
        // Point the camera at approximately the center of the table
        go.transform.forward = -(go.transform.position - Vector3.zero + table_outer_length * 0.15f * Random.insideUnitSphere);
        go.transform.Rotate(go.transform.forward, (Random.value - 0.5f) * 15.0f);
        this.GetComponent<TableManager>().agents[0].SetCamera(go.GetComponent<Camera>());

        go = new GameObject("Camera Player 2");
        go.AddComponent<Camera>();
        go.transform.position = new Vector3(0f, table_outer_length / 2.0f, 0f) +
                                new Vector3(table_outer_length * 1.5f / 2.0f, 0f, 0f) +
                                table_outer_length * 0.1f * Random.insideUnitSphere;
        go.transform.transform.parent = table.transform;
        // Point the camera at approximately the center of the table
        go.transform.forward = -(go.transform.position - Vector3.zero + table_outer_length * 0.15f * Random.insideUnitSphere);
        go.transform.Rotate(go.transform.forward, (Random.value - 0.5f) * 15.0f);
        this.GetComponent<TableManager>().agents[1].SetCamera(go.GetComponent<Camera>());



        /*
        // Place the light randomly
        go = new GameObject("Light");
        Light lightComp = go.AddComponent<Light>();
        lightComp.color = Random.ColorHSV();
        if( Random.value > 0.5f )
            lightComp.shadows = LightShadows.Hard;
        else
            lightComp.shadows = LightShadows.Soft;
           

        go.transform.position = new Vector3(0f, table_full_height, 0f) + table_full_height * 0.5f * Random.insideUnitSphere;
        go.transform.transform.parent = foosball_setup.transform;
        */

        // Build the rods
        List<ROD> rods_info = new List<ROD>()
        {
            new ROD("Player 1: goalie", 0, 3, rod_goalie_player_spacing),
            new ROD("Player 1: defense", 0, 2, rod_2bar_player_spacing),
            new ROD("Player 2: 3-bar", 1, 3, rod_3bar_player_spacing),
            new ROD("Player 1: 5-bar", 0, 5, rod_5bar_player_spacing),
            new ROD("Player 2: 5-bar", 1, 5, rod_5bar_player_spacing),
            new ROD("Player 1: 3-bar", 0, 3, rod_3bar_player_spacing),
            new ROD("Player 2: defense", 1, 2, rod_2bar_player_spacing),
            new ROD("Player 2: goalie", 1, 3, rod_goalie_player_spacing),
        };

        // Calculate distance between each rod based on player height
        float player_toes_to_toes = (table_inner_length - player_goalie_toes_to_wall * 2.0f - player_rod_to_toes * 16.0f) / 7.0f;
        float rod_spacing = 2.0f * player_rod_to_toes + player_toes_to_toes;

        PlayerAgent player1_manager = this.GetComponent<TableManager>().player1_manager;
        PlayerAgent player2_manager = this.GetComponent<TableManager>().player2_manager;

        for ( int i = 0; i < rods_info.Count; i++ )
        {
            // Build this rod
            //  x -> direction of play
            //  y -> vertical
            //  z -> direction of rods
            ROD rod_info = rods_info[i];

            // Calculate rod length
            float rod_travel = (table_inner_width
                            - rod_info.rod_player_count * player_width
                            - 2.0f * bumper_length
                            - (rod_info.rod_player_count - 1) * rod_info.rod_player_spacing);

            float rod_length = table_outer_width + rod_travel + rod_handle_length + rod_handle_to_table;

            // Add the rod bearings
            go = GameObject.CreatePrimitive(PrimitiveType.Cylinder); // note cylinder is 2.0 units in y direction by default
            go.name = "Rod" + i + " Bearing1";
            go.layer = (int)PHYSICS_LAYERS.Table;
            go.GetComponent<Renderer>().material = material_bumper;
            go.transform.localScale = new Vector3(bumper_diameter*1.5f, rod_bearing_width/2.0f, bumper_diameter * 1.5f);
            go.transform.Rotate(new Vector3(1f, 0, 0), 90.0f);
            go.transform.position = new Vector3(rod_spacing * (i - 4.0f + 0.5f), rod_surface_to_rod, -(table_inner_width + table_outer_width) / 4.0f);
            go.transform.transform.parent = table.transform;

            go = GameObject.CreatePrimitive(PrimitiveType.Cylinder); // note cylinder is 2.0 units in y direction by default
            go.name = "Rod" + i + " Bearing2";
            go.layer = (int)PHYSICS_LAYERS.Table ;
            go.GetComponent<Renderer>().material = material_bumper;
            go.transform.localScale = new Vector3(bumper_diameter * 1.5f, rod_bearing_width/2.0f, bumper_diameter * 1.5f);
            go.transform.Rotate(new Vector3(1f, 0, 0), 90.0f);
            go.transform.position = new Vector3(rod_spacing * (i - 4.0f + 0.5f), rod_surface_to_rod, +(table_inner_width + table_outer_width) / 4.0f);
            go.transform.transform.parent = table.transform;

            // Rod base object
            GameObject rod = new GameObject{name = rod_info.rod_name};
            rod.tag = "Rod";
            rod.transform.transform.parent = table.transform;
            rod.layer = (int)PHYSICS_LAYERS.Rods;
            int rod_number = 0;
            if (rod_info.player == 0)
            {
                rod_number = player1_manager.rods.Count;
                player1_manager.rods.Add(rod);
            }
            else
            {
                rod_number = 3 - player2_manager.rods.Count;
                player2_manager.rods.Insert(0, rod);
            }
            

            // Add the rod ball detectors
            go = GameObject.CreatePrimitive(PrimitiveType.Cylinder); // note cylinder is 2.0 units in y direction by default
            go.name = "Rod Ball Detector";
            go.layer = (int)PHYSICS_LAYERS.BallDetector;
            go.GetComponent<CapsuleCollider>().isTrigger = true; // Now it will only detect the ball
            go.GetComponent<MeshRenderer>().enabled = false;  // No need to render this
            RodBallTracking rbt = go.AddComponent<RodBallTracking>();
            rbt.player = rod_info.player;
            rbt.rod = rod_number; // TODO: Fix
            rbt.tableManager = this.GetComponent<TableManager>();
            go.transform.localScale = new Vector3(player_rod_to_toes * 2.0f + ball_diameter - delta, table_inner_width/2.0f, player_rod_to_toes*2.0f + ball_diameter - delta);
            go.transform.Rotate(new Vector3(1f, 0, 0), 90.0f);
            go.transform.position = new Vector3(rod_spacing * (i - 4.0f + 0.5f), rod_surface_to_rod, 0f);
            go.transform.transform.parent = table.transform;


            // Cylinder rod
            go = GameObject.CreatePrimitive(PrimitiveType.Cylinder); // note cylinder is 2.0 units in y direction by default
            go.name = "Rod";
            go.layer = (int)PHYSICS_LAYERS.Rods;
            go.GetComponent<Renderer>().material = material_rod;
            go.transform.localScale = new Vector3(rod_diameter, rod_length/2.0f, rod_diameter);
            go.transform.Rotate(new Vector3(1f, 0, 0), 90.0f);
            go.transform.transform.parent = rod.transform;

            // Handle
            go = GameObject.CreatePrimitive(PrimitiveType.Cylinder); // note cylinder is 2.0 units in y direction by default
            go.name = "Handle";
            go.layer = (int)PHYSICS_LAYERS.Rods;
            go.GetComponent<Renderer>().material = material_handle[rod_info.player];
            go.transform.localScale = new Vector3(rod_handle_diameter, rod_handle_length/2.0f, rod_handle_diameter);
            go.transform.Rotate(new Vector3(1f, 0, 0), 90.0f);
            go.transform.position = new Vector3(0.0f, 0.0f, -rod_length / 2.0f
                                                        + rod_handle_length / 2.0f);
            go.transform.transform.parent = rod.transform;

            // Bumper cylinders
            go = GameObject.CreatePrimitive(PrimitiveType.Cylinder); // note cylinder is 2.0 units in y direction by default
            go.name = "Bumper1";
            go.layer = (int)PHYSICS_LAYERS.Rods;
            go.GetComponent<Renderer>().material = material_bumper;
            go.transform.localScale = new Vector3(bumper_diameter, bumper_length / 2.0f, bumper_diameter);
            go.transform.Rotate(new Vector3(1f, 0, 0), 90.0f);
            go.transform.position = new Vector3(0.0f, 0.0f, -rod_length / 2.0f
                                                        + rod_handle_length
                                                        + rod_handle_to_table
                                                        + (table_outer_width - table_inner_width) / 2.0f
                                                        - bumper_length/8.0f // hackjob, why is this needed? Seems to fix it.
                                                        + rod_travel);
            go.transform.transform.parent = rod.transform;

            
            go = GameObject.CreatePrimitive(PrimitiveType.Cylinder); // note cylinder is 2.0 units in y direction by default
            go.name = "Bumper2";
            go.layer = (int)PHYSICS_LAYERS.Rods;
            go.GetComponent<Renderer>().material = material_bumper;
            go.transform.localScale = new Vector3(bumper_diameter, bumper_length / 2.0f, bumper_diameter);
            go.transform.Rotate(new Vector3(1f, 0, 0), 90.0f);
            go.transform.position = new Vector3(0.0f, 0.0f, -rod_length/2.0f
                                                        + rod_handle_length
                                                        + rod_handle_to_table
                                                        + (table_outer_width - table_inner_width) / 2.0f
                                                        - bumper_length / 8.0f // hackjob, why is this needed? Seems to fix it.
                                                        + rod_travel
                                                        + bumper_length
                                                        + (rod_info.rod_player_count - 1) * rod_info.rod_player_spacing
                                                        + rod_info.rod_player_count * player_width);
            go.transform.transform.parent = rod.transform;

            // Build the players
            for (int j = 0; j < rod_info.rod_player_count; j++)
            {
                go = BuildPlayerGameObject(material_player[rod_info.player], material_foot_physics);
                go.layer = (int)PHYSICS_LAYERS.Rods;
                float player_position = (-rod_length / 2.0f
                                     + rod_handle_length 
                                     + rod_handle_to_table
                                     + (table_outer_width - table_inner_width) / 2.0f
                                     + bumper_length
                                     + rod_travel
                                     + j * (rod_info.rod_player_spacing + player_width));
                go.transform.position = new Vector3(0.0f, 0.0f, player_position);
                go.transform.parent = rod.transform;
            }

            // Set up the rod joint physics
            rb = rod.AddComponent<Rigidbody>();
            rb.useGravity = true;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            //rb.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY;
            rb.drag = 2.0f * Random.Range(0.80f, 1.2f);
            rb.angularDrag = 0.5f * Random.Range(0.8f, 1.2f);
            rb.maxAngularVelocity = 2f * 3.14f * 1000f;

            // Add the mass
            float mass = (rod_mass + rod_info.rod_player_count * player_mass) * Random.Range(0.80f, 1.20f);
            rb.mass = mass;


            // Position the rod and flip it if it's player 2
            // 1 2 3 4 5 6 7 8
            if (rod_info.player == 0)
            {
                rod.transform.position = new Vector3(rod_spacing * (i - 4.0f + 0.5f), rod_surface_to_rod, -rod_length / 2.0f + table_outer_width / 2.0f + rod_handle_to_table / 2.0f + rod_travel/2.0f);
            }
            else
            {
                rod.transform.Rotate(new Vector3(0, 1f, 0), 180.0f);
                rod.transform.position = new Vector3(rod_spacing * (i - 4.0f + 0.5f), rod_surface_to_rod, -(-rod_length / 2.0f + table_outer_width / 2.0f + rod_handle_to_table / 2.0f + rod_travel / 2.0f));
            }


            

            // Add the rod limits
            ConfigurableJoint cj = rod.AddComponent<ConfigurableJoint>();
            cj.connectedBody = table.GetComponent<Rigidbody>(); // Attach reference to the table
            cj.xMotion = ConfigurableJointMotion.Locked;
            cj.yMotion = ConfigurableJointMotion.Locked;
            cj.zMotion = ConfigurableJointMotion.Limited;
            cj.angularXMotion = ConfigurableJointMotion.Locked;
            cj.angularYMotion = ConfigurableJointMotion.Locked;
            cj.angularZMotion = ConfigurableJointMotion.Free;
            
            cj.linearLimit = new SoftJointLimit
            {
                limit = rod_travel / 2.0f
            };
            cj.linearLimitSpring = new SoftJointLimitSpring
            {
                spring = 1000f * mass * Random.Range(0.25f, 3.0f),
                damper = 1f * mass * Random.Range(0.25f, 3.0f)
            };



            /*
            SpringJoint sj = rod.AddComponent<SpringJoint>();
            //cj.connectedBody = table.GetComponent<Rigidbody>(); // Attach reference to the table
            sj.spring = 100.0f;
            sj.damper = 0.8f;
            sj.minDistance = 0.0f;
            sj.maxDistance = rod_travel / 2.0f;
            sj.autoConfigureConnectedAnchor = true;
            */


            // Build this rod's ball detector box


        }

        table.transform.position = gameObject.transform.position;
    }

    GameObject BuildPlayerGameObject(Material material, PhysicMaterial material_foot_physics)
    {
        // Estimated masses in grams
        float bumper_diameter = 0.32f; // (cm). Diameter of rod bumper
        float player_chest_width = 0.32f; // (cm). Player width from the side view, the diameter of the chest part that the rod goes through
        float player_toes_width = 0.22f; // (cm). Foot width of the player feet (along the axis of the rod)
        float player_rod_to_head = 0.35f; // (cm). 
        float player_rod_to_toes = 0.73f; // (cm). 
        float player_width = 0.3f; // (cm). Player width of a player mid-center along axis of the rod. Basically measure the length of the hole in the player.
        float player_foot_width_forward = 0.5f * 0.7f * player_chest_width;
        float player_foot_width_backward = 0.7f * 0.7f * player_chest_width;
        float player_foot_toes_forward = 0.2f * player_chest_width; // (cm). 
        float player_foot_height = player_toes_width*1.4f;

        GameObject player = new GameObject();
        player.name = "Player";
        //  x -> direction of play
        //  y -> vertical
        //  z -> direction of rods

        // Build the horizontal cylinder, this is the center of the player
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cylinder); // note cylinder is 2.0 units in y direction by default
        go.name = "BarCylinder";
        go.layer = (int)PHYSICS_LAYERS.Rods;
        go.GetComponent<Renderer>().material = material;
        go.transform.localScale = new Vector3(bumper_diameter, player_width / 2.0f, bumper_diameter);
        go.transform.Rotate(new Vector3(1f, 0, 0), 90.0f);
        go.transform.transform.parent = player.transform;

        // Build the head cylinder
        go = GameObject.CreatePrimitive(PrimitiveType.Cylinder); // note cylinder is 2.0 units in y direction by default
        go.name = "HeadCylinder";
        go.layer = (int)PHYSICS_LAYERS.Rods;
        go.GetComponent<Renderer>().material = material;
        go.transform.localScale = new Vector3(player_width*0.85f, player_rod_to_head / 2.0f, player_width * 0.85f);
        go.transform.position = new Vector3(0f, player_rod_to_head / 2.0f, 0f);
        go.transform.transform.parent = player.transform;

        // Build the foot cylinder
        go = GameObject.CreatePrimitive(PrimitiveType.Cylinder); // note cylinder is 2.0 units in y direction by default
        go.name = "FootCylinder";
        go.layer = (int)PHYSICS_LAYERS.Rods;
        go.GetComponent<Renderer>().material = material;
        go.transform.localScale = new Vector3(player_width * 0.75f, ((player_rod_to_toes-player_foot_height) / 2.0f), player_width * 0.75f);
        go.transform.position = new Vector3(0f, -((player_rod_to_toes - player_foot_height) / 2.0f), 0f); // Don't shift it all the way down
        go.transform.transform.parent = player.transform;

        // Build the player foot, for now two planes
        List<Vector3> vertices = new List<Vector3>
        {
                // Front of player foot
                new Vector3(player_foot_width_forward, -player_rod_to_toes+player_foot_height, player_toes_width/2.0f ),
                new Vector3(player_foot_width_forward, -player_rod_to_toes+player_foot_height, -player_toes_width/2.0f ),
                new Vector3(player_foot_toes_forward, -player_rod_to_toes, -player_toes_width/2.0f ),
                new Vector3(player_foot_toes_forward, -player_rod_to_toes, player_toes_width/2.0f ),

                // Back of player foot
                new Vector3(-player_foot_width_backward, -player_rod_to_toes+player_foot_height, player_toes_width/2.0f ),
                new Vector3(-player_foot_width_backward, -player_rod_to_toes+player_foot_height, -player_toes_width/2.0f ),
                new Vector3(player_foot_toes_forward, -player_rod_to_toes, -player_toes_width/2.0f ),
                new Vector3(player_foot_toes_forward, -player_rod_to_toes, player_toes_width/2.0f ),

                // Top of player foot
                new Vector3(player_foot_width_forward, -player_rod_to_toes+player_foot_height, player_toes_width/2.0f ),
                new Vector3(player_foot_width_forward, -player_rod_to_toes+player_foot_height, -player_toes_width/2.0f ),
                new Vector3(-player_foot_width_backward, -player_rod_to_toes+player_foot_height, -player_toes_width/2.0f ),
                new Vector3(-player_foot_width_backward, -player_rod_to_toes+player_foot_height, player_toes_width/2.0f ),

                // Sides of feet
                new Vector3(player_foot_width_forward, -player_rod_to_toes+player_foot_height, player_toes_width/2.0f ),
                new Vector3(-player_foot_width_backward, -player_rod_to_toes+player_foot_height, player_toes_width/2.0f ),
                new Vector3(player_foot_toes_forward, -player_rod_to_toes, player_toes_width/2.0f ),

                new Vector3(player_foot_width_forward, -player_rod_to_toes+player_foot_height, -player_toes_width/2.0f ),
                new Vector3(player_foot_toes_forward, -player_rod_to_toes, -player_toes_width/2.0f ),
                new Vector3(-player_foot_width_backward, -player_rod_to_toes+player_foot_height, -player_toes_width/2.0f ),
        };
        List<int> triangles = GetTrianglesFromSquares(vertices.GetRange(0,12));
        triangles.AddRange( new List<int>() { 12,13,14,15,16,17 } );
        List<Vector3> normals = GetNormalsFromSquares(vertices.GetRange(0, 12));
        normals.AddRange(GetNormalsFromTriangles(vertices.GetRange(12, 6)));
        

        go = new GameObject("Foot", typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))
        {
            layer = (int)PHYSICS_LAYERS.Rods
        };
        Mesh m = new Mesh();
        go.GetComponent<MeshCollider>().sharedMesh = m;
        go.GetComponent<MeshCollider>().convex = true;
        go.GetComponent<MeshCollider>().material = material_foot_physics;
        go.GetComponent<MeshFilter>().mesh = m;
        m.vertices = vertices.ToArray();
        m.triangles = triangles.ToArray();
        m.normals = normals.ToArray();
        go.GetComponent<Renderer>().material = material;
        go.transform.transform.parent = player.transform;

        return player;
    }

    List<int> GetTrianglesFromSquares(List<Vector3> vertices)
    {
        List<int> triangles = new List<int>(vertices.Count*2);
        for(int i = 0; i < vertices.Count; i+=4)
        {
            triangles.Add(i);
            triangles.Add(i+1);
            triangles.Add(i+2);

            triangles.Add(i + 2);
            triangles.Add(i + 1);
            triangles.Add(i);
            
            
            triangles.Add(i+2);
            triangles.Add(i+3);
            triangles.Add(i);

            triangles.Add(i);
            triangles.Add(i + 3);
            triangles.Add(i + 2);
        }
        return triangles;
    }

    List<Vector3> GetNormalsFromSquares(List<Vector3> vertices)
    {
        // Each square has a normal
        List<Vector3> normals = new List<Vector3>(vertices.Count);
        for (int i = 0; i < vertices.Count; i += 4)
        {
            Vector3 normal = Vector3.Cross(vertices[i + 1] - vertices[i], vertices[i+2] - vertices[i]);
            normals.Add(normal);
            normals.Add(normal);
            normals.Add(normal);
            normals.Add(normal);
        }
        return normals;
    }

    List<Vector3> GetNormalsFromTriangles(List<Vector3> vertices)
    {
        // Each square has a normal
        List<Vector3> normals = new List<Vector3>(vertices.Count);
        for (int i = 0; i < vertices.Count; i += 3)
        {
            Vector3 normal = Vector3.Cross(vertices[i + 1] - vertices[i], vertices[i + 2] - vertices[i]);
            normals.Add(normal);
            normals.Add(normal);
            normals.Add(normal);
        }
        return normals;
    }

    void BuildTable(float table_OuterWidth, float table_InnerWidth, float table_Height, float table)
    {

    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
