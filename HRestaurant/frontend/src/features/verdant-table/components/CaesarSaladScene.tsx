import { useEffect, useRef } from "react";
import * as THREE from "three";

type FallingIngredient = { mesh: THREE.Mesh; speed: number; rotateX: number; rotateY: number };
const lettuceColors = [0x4d9244, 0x76b852, 0xaed581];
const positionIngredient = (mesh: THREE.Mesh, y = Math.random() * 6 + 1) => mesh.position.set((Math.random() - .5) * 6, y, (Math.random() - .5) * 6);

export function CaesarSaladScene() {
  const containerRef = useRef<HTMLDivElement>(null);
  useEffect(() => {
    const container = containerRef.current;
    if (!container) return;
    const scene = new THREE.Scene();
    const camera = new THREE.PerspectiveCamera(75, 1, .1, 1000);
    camera.position.set(0, 8, 12); camera.lookAt(0, 0, 0);
    const renderer = new THREE.WebGLRenderer({ antialias: true, alpha: true });
    renderer.setPixelRatio(Math.min(window.devicePixelRatio || 1, 2));
    renderer.setClearColor(0x000000, 0);
    container.replaceChildren(renderer.domElement);
    scene.add(new THREE.AmbientLight(0xffffff, .8));
    const directionalLight = new THREE.DirectionalLight(0xffffff, 1.2); directionalLight.position.set(5, 10, 7.5); scene.add(directionalLight);
    const bowl = new THREE.Group();
    const bowlMaterial = new THREE.MeshStandardMaterial({ color: 0xfaf9f4, roughness: .32, metalness: .02, side: THREE.DoubleSide });
    const bowlSide = new THREE.Mesh(new THREE.CylinderGeometry(4, 2.5, 2, 32, 1, true), bowlMaterial); bowlSide.position.y = -.55; bowl.add(bowlSide);
    const bowlBottom = new THREE.Mesh(new THREE.CircleGeometry(2.5, 32), bowlMaterial); bowlBottom.rotation.x = -Math.PI / 2; bowlBottom.position.y = -1.55; bowl.add(bowlBottom); scene.add(bowl);
    const ingredients: FallingIngredient[] = [];
    const addIngredient = (geometry: THREE.BufferGeometry, material: THREE.Material) => { const mesh = new THREE.Mesh(geometry, material); positionIngredient(mesh); mesh.rotation.set(Math.random() * Math.PI, Math.random() * Math.PI, 0); bowl.add(mesh); ingredients.push({ mesh, speed: .012 + Math.random() * .024, rotateX: .01 + Math.random() * .025, rotateY: .01 + Math.random() * .025 }); };
    Array.from({ length: 20 }, () => { const size = .3 + Math.random() * .4; addIngredient(new THREE.IcosahedronGeometry(size, 1), new THREE.MeshStandardMaterial({ color: lettuceColors[Math.floor(Math.random() * lettuceColors.length)], flatShading: true })); });
    Array.from({ length: 10 }, () => addIngredient(new THREE.BoxGeometry(.3, .3, .3), new THREE.MeshStandardMaterial({ color: 0xd4a373, roughness: .7 })));
    Array.from({ length: 6 }, () => addIngredient(new THREE.SphereGeometry(.25, 16, 16), new THREE.MeshStandardMaterial({ color: 0xe63946, roughness: .45 })));
    const resize = () => { const { width, height } = container.getBoundingClientRect(); camera.aspect = Math.max(width / Math.max(height, 1), .1); camera.updateProjectionMatrix(); renderer.setSize(width, height, false); };
    const observer = new ResizeObserver(resize); observer.observe(container); resize();
    let frame = 0;
    const animate = () => { frame = requestAnimationFrame(animate); bowl.rotation.y += .003; ingredients.forEach(({ mesh, speed, rotateX, rotateY }) => { mesh.position.y -= speed; mesh.rotation.x += rotateX; mesh.rotation.y += rotateY; if (mesh.position.y < -.5) positionIngredient(mesh, 5); }); renderer.render(scene, camera); };
    animate();
    return () => { cancelAnimationFrame(frame); observer.disconnect(); scene.traverse((object) => { if (object instanceof THREE.Mesh) { object.geometry.dispose(); const materials = Array.isArray(object.material) ? object.material : [object.material]; materials.forEach((material) => material.dispose()); } }); renderer.dispose(); renderer.forceContextLoss(); renderer.domElement.remove(); };
  }, []);
  return <div ref={containerRef} aria-label="Fırlanan Caesar salatı 3D animasiyası" className="h-[21rem] w-full sm:h-[27rem] lg:h-[34rem]" />;
}
