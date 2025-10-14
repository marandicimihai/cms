// Moving Multi-Blob Purple Nebula Background
(function() {
  const canvas = document.getElementById("nebula-canvas");
  if (!canvas) return;
  
  const ctx = canvas.getContext("2d", { alpha: true });
  let W, H;
  
  function resize() {
    W = window.innerWidth;
    H = window.innerHeight;
    canvas.width = W;
    canvas.height = H;
  }
  resize();
  window.addEventListener("resize", resize);

  function rand(a, b) { return a + Math.random() * (b - a); }
  function lerp(a, b, t) { return a + (b - a) * t; }

  const baseColor = { r: 144, g: 31, b: 115 };

  const blobs = [];
  const NUM_BLOBS = 6;
  for (let i = 0; i < NUM_BLOBS; i++) {
    blobs.push({
      x: rand(0, 1),
      y: rand(0, 1),
      r: rand(0.15, 0.35),
      vx: rand(-0.00005, 0.00005),
      vy: rand(-0.00005, 0.00005),
      targetVX: 0,
      targetVY: 0,
      color: { r: baseColor.r, g: baseColor.g, b: baseColor.b, a: rand(0.15, 0.35) },
      changeTimer: rand(2000, 7000)
    });
  }

  function updateBlobs(dt) {
    blobs.forEach(b => {
      b.changeTimer -= dt;
      if (b.changeTimer <= 0) {
        // choose new drift direction every few seconds
        b.targetVX = rand(-0.00008, 0.00008);
        b.targetVY = rand(-0.00008, 0.00008);
        b.changeTimer = rand(3000, 9000);
      }
      // smooth velocity change
      b.vx = lerp(b.vx, b.targetVX, 0.01);
      b.vy = lerp(b.vy, b.targetVY, 0.01);

      b.x += b.vx * dt;
      b.y += b.vy * dt;

      // wrap around edges
      if (b.x < -0.3) b.x = 1.3;
      if (b.x > 1.3) b.x = -0.3;
      if (b.y < -0.3) b.y = 1.3;
      if (b.y > 1.3) b.y = -0.3;
    });
  }

  function drawBlobs() {
    ctx.globalCompositeOperation = "lighter";
    blobs.forEach(b => {
      const px = b.x * W;
      const py = b.y * H;
      const radius = Math.max(W, H) * b.r;
      const g = ctx.createRadialGradient(px, py, radius * 0.1, px, py, radius);
      g.addColorStop(0, `rgba(${b.color.r},${b.color.g},${b.color.b},${b.color.a})`);
      g.addColorStop(1, "rgba(8,7,8,0)");
      ctx.fillStyle = g;
      ctx.fillRect(px - radius, py - radius, radius * 2, radius * 2);
    });
    ctx.globalCompositeOperation = "source-over";
  }

  function drawVignette() {
    const vg = ctx.createRadialGradient(W * 0.5, H * 0.5, Math.min(W, H) * 0.3, W * 0.5, H * 0.5, Math.max(W, H) * 0.9);
    vg.addColorStop(0, "rgba(0,0,0,0)");
    vg.addColorStop(0.7, "rgba(0,0,0,0.25)");
    vg.addColorStop(1, "rgba(0,0,0,0.75)");
    ctx.fillStyle = vg;
    ctx.fillRect(0, 0, W, H);
  }

  function drawStar() {
    const size = Math.min(W, H) * 0.03;
    const cx = W - size * 0.8;
    const cy = H - size * 0.8;
    ctx.save();
    ctx.translate(cx, cy);
    ctx.beginPath();
    ctx.moveTo(0, -size * 0.5);
    ctx.lineTo(size * 0.5, 0);
    ctx.lineTo(0, size * 0.5);
    ctx.lineTo(-size * 0.5, 0);
    ctx.closePath();
    ctx.fillStyle = "rgba(230,230,230,0.95)";
    ctx.fill();
    ctx.beginPath();
    ctx.arc(0, 0, size * 0.9, 0, Math.PI * 2);
    ctx.fillStyle = "rgba(255,255,255,0.04)";
    ctx.fill();
    ctx.restore();
  }

  function drawNoise(intensity) {
    const imageData = ctx.getImageData(0, 0, W, H);
    const data = imageData.data;
    for (let i = 0; i < data.length; i += 4) {
      const n = (Math.random() - 0.5) * 255 * intensity;
      data[i] += n; data[i + 1] += n; data[i + 2] += n;
    }
    ctx.putImageData(imageData, 0, 0);
  }

  let last = performance.now();
  function animate(now) {
    const dt = now - last;
    last = now;

    ctx.fillStyle = "#080408";
    ctx.fillRect(0, 0, W, H);

    updateBlobs(dt);
    drawBlobs();
    drawVignette();
    drawStar();
    drawNoise(0.04);

    requestAnimationFrame(animate);
  }
  requestAnimationFrame(animate);
})();
