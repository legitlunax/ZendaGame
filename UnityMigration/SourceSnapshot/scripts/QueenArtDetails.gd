class_name QueenArtDetails
extends RefCounted

# Original geometry, not a cutout or projection of the supplied illustration.
static func curved_points(points: Array[Vector3], steps: int = 5) -> Array[Vector3]:
	var result: Array[Vector3] = []
	for index: int in range(points.size() - 1):
		for step: int in range(steps):
			result.append(points[index].cubic_interpolate(points[index + 1], points[maxi(0, index - 1)], points[mini(points.size() - 1, index + 2)], float(step) / float(steps)))
	result.append(points.back())
	return result

static func curved_widths(widths: Array[float], steps: int = 5) -> Array[float]:
	var result: Array[float] = []
	for index: int in range(widths.size() - 1):
		for step: int in range(steps):
			result.append(lerpf(widths[index], widths[index + 1], float(step) / float(steps)))
	result.append(widths.back())
	return result

static func strand(parent: Node3D, points: Array[Vector3], widths: Array[float], material: Material) -> MeshInstance3D:
	points = curved_points(points)
	widths = curved_widths(widths)
	var surface: SurfaceTool = SurfaceTool.new()
	surface.begin(Mesh.PRIMITIVE_TRIANGLES)
	for row: int in range(points.size() - 1):
		for segment: int in range(24):
			var vertices: Array[Vector3] = []
			for offset: Vector2i in [Vector2i(0, 0), Vector2i(1, 0), Vector2i(1, 1), Vector2i(0, 1)]:
				var index: int = row + offset.y
				var angle: float = TAU * float(segment + offset.x) / 24.0
				vertices.append(points[index] + Vector3(cos(angle) * widths[index], 0, sin(angle) * widths[index] * 0.42))
			for vertex: int in [0, 2, 1, 0, 3, 2]:
				surface.add_vertex(vertices[vertex])
	surface.generate_normals()
	var mesh: MeshInstance3D = MeshInstance3D.new()
	mesh.mesh = surface.commit()
	mesh.material_override = material
	parent.add_child(mesh)
	return mesh

static func ribbon(parent: Node3D, points: Array[Vector3], widths: Array[float], material: Material) -> MeshInstance3D:
	points = curved_points(points)
	widths = curved_widths(widths)
	var surface: SurfaceTool = SurfaceTool.new()
	surface.begin(Mesh.PRIMITIVE_TRIANGLES)
	for row: int in range(points.size() - 1):
		var vertices: Array[Vector3] = [points[row] - Vector3(widths[row], 0, 0), points[row] + Vector3(widths[row], 0, 0), points[row + 1] + Vector3(widths[row + 1], 0, 0), points[row + 1] - Vector3(widths[row + 1], 0, 0)]
		for vertex: int in [0, 1, 2, 0, 2, 3]:
			surface.add_vertex(vertices[vertex])
	surface.generate_normals()
	var mesh: MeshInstance3D = MeshInstance3D.new()
	mesh.mesh = surface.commit()
	mesh.material_override = material
	parent.add_child(mesh)
	return mesh