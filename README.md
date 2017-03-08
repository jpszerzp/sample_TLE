# sample_TLE

This is a repo containing sample codes of private The Last Engineer game project. The repo now contains complete set of files for intermediate boss fight. Tested with version 5.4.4 of Unity3D.

## Brief Documentation

Utility
----------
### Singleton.cs
Singleton module used in this project.

### MonoExtension.cs
Attempt at extending various utility modules based on existing ones of Unity3D.

### MovementTest.cs
Test package for movements.

### Testplayer.cs
Include tests on avatar specifically.

Player Module
-------------
### Player.cs
Encapsulation of avatar the player character.

Boss Module
------------
### NewMidboss.cs
Instance of Midboss defining its major functions and behaviors. Midboss.cs is deprecated.

### MidbossManager.cs
Boss manager singleton that governs states of boss instance in game scene. BossManager.cs is deprecated.

Weapon & Attack Module
------------------------
### Bullets.cs
Encapsulation of bullet attack. Bullet.cs is deprecated.

### ElectricBall.cs
Encapsulation of electrical ball attack.

### Rocket.cs
Encapsulation of rocket attack.

### GroundSpot.cs
Encapsulation of projection of rocket on ground. Handles explosion separation separately.

