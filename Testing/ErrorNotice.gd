extends Node

@onready var error_icon = preload("res://Resources/ErrorNotice.tscn").instantiate()
func _ready():
	add_child(error_icon)
	error_icon.position = Vector2(500, 500)
