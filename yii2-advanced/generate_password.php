<?php
// Khởi tạo Yii2 application
defined('YII_DEBUG') or define('YII_DEBUG', true);
defined('YII_ENV') or define('YII_ENV', 'dev');

require __DIR__ . '/vendor/autoload.php';
require __DIR__ . '/vendor/yiisoft/yii2/Yii.php';
require __DIR__ . '/common/config/bootstrap.php';
require __DIR__ . '/console/config/bootstrap.php';

$config = yii\helpers\ArrayHelper::merge(
    require __DIR__ . '/common/config/main.php',
    require __DIR__ . '/common/config/main-local.php',
    require __DIR__ . '/console/config/main.php',
    require __DIR__ . '/console/config/main-local.php'
);

$application = new yii\console\Application($config);

// Generate password
$password = 'admin123';
$hash = Yii::$app->security->generatePasswordHash($password);
$authKey = Yii::$app->security->generateRandomString();

echo "Password: $password\n";
echo "Password Hash: $hash\n";
echo "Auth Key: $authKey\n";
echo "\n\nSQL Command:\n";
echo "UPDATE `user` SET `password_hash` = '$hash', `auth_key` = '$authKey' WHERE `username` = 'admin';\n";