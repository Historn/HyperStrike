<?php
// Database connection
$host = "localhost";
$username = "joelcm";
$password = "47240087l";
$database = "joelcm";

$conn = new mysqli($host, $username, $password, $database);
if ($conn->connect_error) {
    die("Connection failed: " . $conn->connect_error);
}

// Check request type
$requestType = $_GET['request'] ?? 'insert';

if ($requestType === 'insert') {
    // Handle data insertion
    $json = file_get_contents("php://input");

    if ($json === false || empty($json)) {
        echo "Error: No JSON input received.";
        exit;
    }

    $data = json_decode($json, true);

    if (!is_array($data)) {
        echo "Error: Invalid JSON input.";
        exit;
    }

    $eventType = $data['EventType'] ?? null;
    $timestamp = $data['Timestamp'] ?? null;
    $position = $data['Position'] ?? null;

    $enemyEventType = $data['EnemyEventType'] ?? null;
    $enemyTimestamp = $data['EnemyTimestamp'] ?? null;
    $enemyPosition = $data['EnemyPosition'] ?? null;

    if ($eventType && $timestamp && $position) {
        $positionEscaped = $conn->real_escape_string($position);

        $sql = $conn->prepare("INSERT INTO events (EventType, Timestamp, Position) VALUES (?, ?, ?)");
        $sql->bind_param("sss", $eventType, $timestamp, $positionEscaped);

        if ($sql->execute()) {
            echo "Data inserted successfully.";
        } else {
            echo "Error: " . $sql->error;
        }
    } 
    else if ($enemyEventType && $enemyTimestamp && $enemyPosition) {
        $enemyPositionEscaped = $conn->real_escape_string($enemyPosition);
        
        $sql = $conn->prepare("INSERT INTO enemyEvents (EnemyEventType, EnemyTimestamp, EnemyPosition) VALUES (?, ?, ?)");
        $sql->bind_param("sss", $enemyEventType, $enemyTimestamp, $enemyPosition);

        if ($sql->execute()) {
            echo "Enemy data inserted successfully.";
        } else {
            echo "Error: " . $sql->error;
        }
    } else {
        echo "Error: Missing required fields.";
    }
} else if ($requestType === 'fetch') {
    // Fetch data for PLAYER heatmap
    $eventType = $_GET['eventType'] ?? '';
    if (empty($eventType)) {
        echo "Error: Missing 'eventType' parameter.";
        exit;
    }

    $sql = "SELECT Position, COUNT(*) as Count FROM events WHERE EventType = ? GROUP BY Position";
    $stmt = $conn->prepare($sql);
    $stmt->bind_param("s", $eventType);
    $stmt->execute();

    $result = $stmt->get_result();
    $output = [];
    while ($row = $result->fetch_assoc()) {
        $output[] = $row;
    }

    echo json_encode($output);
} else if ($requestType === 'fetchEnemies') {
    // Fetch data for ENEMIES heatmap
    $enemyEventType = $_GET['enemyEventType'] ?? '';
    if (empty($enemyEventType)) {
        echo "Error: Missing 'enemyEventType' parameter.";
        exit;
    }

    $sql = "SELECT EnemyPosition, COUNT(*) as Count FROM enemyEvents WHERE EnemyEventType = ? GROUP BY EnemyPosition";
    $stmt = $conn->prepare($sql);
    $stmt->bind_param("s", $enemyEventType);
    $stmt->execute();

    $result = $stmt->get_result();
    $output = [];
    while ($row = $result->fetch_assoc()) {
        $output[] = $row;
    }

    echo json_encode($output);
} 
else {
    echo "Error: Invalid request type.";
}

$conn->close();
?>
