"use client";

import { useEffect, useRef } from "react";

type Ripple = { x: number; y: number; start: number };

const GAP = 30;
const RIPPLE_MS = 1700;
const CURSOR_RADIUS = 150;

/**
 * Decorative dotted background. The grid reacts softly to the cursor and sends a gentle ripple when the pointer
 * moves or presses. Touch devices, narrow screens, and prefers-reduced-motion get a static grid with no animation
 * loop and no listeners.
 */
export function DotField() {
  const canvasRef = useRef<HTMLCanvasElement>(null);

  useEffect(() => {
    const canvas = canvasRef.current;
    const ctx = canvas?.getContext("2d");
    if (!canvas || !ctx) return;

    const reduce = window.matchMedia("(prefers-reduced-motion: reduce)");
    const coarse = window.matchMedia("(pointer: coarse)");
    const narrow = window.matchMedia("(max-width: 767px)");

    let width = 0;
    let height = 0;
    let frame = 0;
    let ripples: Ripple[] = [];
    const pointer = { x: -9999, y: -9999, lastMove: 0, lastRipple: 0, lastRippleX: -9999, lastRippleY: -9999 };

    const animated = () => !reduce.matches && !coarse.matches && !narrow.matches;

    function draw(now: number) {
      frame = 0;
      if (!ctx) return;
      ctx.clearRect(0, 0, width, height);
      const live = animated();
      ripples = ripples.filter((ripple) => now - ripple.start < RIPPLE_MS);

      for (let gx = GAP / 2; gx < width + GAP; gx += GAP) {
        for (let gy = GAP / 2; gy < height + GAP; gy += GAP) {
          let x = gx;
          let y = gy;
          let alpha = 0.26;
          let radius = 1.2;

          if (live) {
            const dx = gx - pointer.x;
            const dy = gy - pointer.y;
            const distance = Math.hypot(dx, dy);
            if (distance < CURSOR_RADIUS && distance > 0.1) {
              const k = 1 - distance / CURSOR_RADIUS;
              alpha += k * 0.45;
              radius += k * 1.3;
              x += (dx / distance) * k * 5;
              y += (dy / distance) * k * 5;
            }

            for (const ripple of ripples) {
              const age = (now - ripple.start) / RIPPLE_MS;
              const rx = gx - ripple.x;
              const ry = gy - ripple.y;
              const rd = Math.hypot(rx, ry);
              const ring = age * 280;
              const delta = Math.abs(rd - ring);
              if (delta < 38 && rd > 0.1) {
                const k = (1 - delta / 38) * (1 - age);
                alpha += k * 0.5;
                radius += k * 1.2;
                x += (rx / rd) * k * 4;
                y += (ry / rd) * k * 4;
              }
            }
          }

          ctx.fillStyle = `rgba(134, 239, 190, ${Math.min(alpha, 0.85).toFixed(3)})`;
          ctx.beginPath();
          ctx.arc(x, y, radius, 0, Math.PI * 2);
          ctx.fill();
        }
      }

      if (live && (ripples.length > 0 || now - pointer.lastMove < 900)) {
        frame = window.requestAnimationFrame(draw);
      }
    }

    function schedule() {
      if (frame === 0) {
        frame = window.requestAnimationFrame(draw);
      }
    }

    function resize() {
      const ratio = Math.min(window.devicePixelRatio || 1, 2);
      width = window.innerWidth;
      height = window.innerHeight;
      canvas!.width = Math.floor(width * ratio);
      canvas!.height = Math.floor(height * ratio);
      canvas!.style.width = `${width}px`;
      canvas!.style.height = `${height}px`;
      ctx!.setTransform(ratio, 0, 0, ratio, 0, 0);
      schedule();
    }

    function spawn(x: number, y: number, now: number) {
      ripples.push({ x, y, start: now });
      if (ripples.length > 5) ripples.shift();
      pointer.lastRipple = now;
      pointer.lastRippleX = x;
      pointer.lastRippleY = y;
    }

    function onMove(event: PointerEvent) {
      if (!animated() || event.pointerType === "touch") return;
      const now = performance.now();
      pointer.x = event.clientX;
      pointer.y = event.clientY;
      pointer.lastMove = now;
      const travelled = Math.hypot(event.clientX - pointer.lastRippleX, event.clientY - pointer.lastRippleY);
      if (travelled > 90 && now - pointer.lastRipple > 420) {
        spawn(event.clientX, event.clientY, now);
      }
      schedule();
    }

    function onDown(event: PointerEvent) {
      if (!animated() || event.pointerType === "touch") return;
      spawn(event.clientX, event.clientY, performance.now());
      schedule();
    }

    function onLeave() {
      pointer.x = -9999;
      pointer.y = -9999;
      schedule();
    }

    resize();
    window.addEventListener("resize", resize);
    window.addEventListener("pointermove", onMove, { passive: true });
    window.addEventListener("pointerdown", onDown, { passive: true });
    document.documentElement.addEventListener("pointerleave", onLeave);
    reduce.addEventListener("change", schedule);
    coarse.addEventListener("change", schedule);
    narrow.addEventListener("change", schedule);

    return () => {
      if (frame) window.cancelAnimationFrame(frame);
      window.removeEventListener("resize", resize);
      window.removeEventListener("pointermove", onMove);
      window.removeEventListener("pointerdown", onDown);
      document.documentElement.removeEventListener("pointerleave", onLeave);
      reduce.removeEventListener("change", schedule);
      coarse.removeEventListener("change", schedule);
      narrow.removeEventListener("change", schedule);
    };
  }, []);

  return <canvas ref={canvasRef} className="ambient-canvas" aria-hidden="true" />;
}
