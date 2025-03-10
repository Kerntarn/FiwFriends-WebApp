let cropCtx, img;
let cropX = 50, cropY = 50, cropSize = 100;
let isDragging = false, isResizing = false, resizeCorner = "";
let startX, startY, startCropX, startCropY, startCropSize;

function openCropPopup(event) {
    const file = event.target.files[0];
    if (!file) return;

    const validExtensions = [".jpg", ".jpeg", ".png", ".gif"];
    const fileExtension = file.name.substring(file.name.lastIndexOf(".")).toLowerCase();

    if (!validExtensions.includes(fileExtension)) {
        alert("Invalid file type. Please upload a JPG, JPEG, PNG, or GIF file.");
        return;
    }

    const reader = new FileReader();
    reader.readAsDataURL(file);

    reader.onload = function (e) {
        img = new Image();
        img.src = e.target.result;

        img.onload = function () {
            cropCanvas = document.getElementById("cropCanvas");
            cropCtx = cropCanvas.getContext("2d");

            const maxWidth = 400;
            const maxHeight = 400;
            const scale = Math.min(maxWidth / img.width, maxHeight / img.height, 1);

            cropCanvas.width = img.width * scale;
            cropCanvas.height = img.height * scale;

            drawImage();
            document.getElementById("cropPopup").style.display = "flex";
        };
    };
}

// วาดรูป + พื้นที่ที่ไม่ได้ Crop ให้มืดลง
function drawImage() {
    cropCtx.clearRect(0, 0, cropCanvas.width, cropCanvas.height);
    cropCtx.drawImage(img, 0, 0, cropCanvas.width, cropCanvas.height);

    // วาด overlay มืดลง
    cropCtx.fillStyle = "rgba(0, 0, 0, 0.5)";
    cropCtx.beginPath();
    cropCtx.rect(0, 0, cropCanvas.width, cropCanvas.height);
    cropCtx.arc(cropX + cropSize / 2, cropY + cropSize / 2, cropSize / 2, 0, Math.PI * 2);
    cropCtx.closePath();
    cropCtx.fill("evenodd");

    drawResizeHandles();
}

// วาดจุด 4 มุมเพื่อใช้ขยาย
function drawResizeHandles() {
    const handles = [
        { x: cropX, y: cropY },
        { x: cropX + cropSize, y: cropY },
        { x: cropX, y: cropY + cropSize },
        { x: cropX + cropSize, y: cropY + cropSize }
    ];
    cropCtx.fillStyle = "white";
    handles.forEach(handle => {
        cropCtx.beginPath();
        cropCtx.arc(handle.x, handle.y, 5, 0, Math.PI * 2);
        cropCtx.fill();
        cropCtx.stroke();
    });
}

// เริ่มลากหรือขยาย
function startInteraction(event) {
    const rect = cropCanvas.getBoundingClientRect();
    startX = event.clientX - rect.left;
    startY = event.clientY - rect.top;
    startCropX = cropX;
    startCropY = cropY;
    startCropSize = cropSize;

    // ตรวจว่ากดที่มุมไหน
    const corners = [
        { x: cropX, y: cropY, name: "top-left" },
        { x: cropX + cropSize, y: cropY, name: "top-right" },
        { x: cropX, y: cropY + cropSize, name: "bottom-left" },
        { x: cropX + cropSize, y: cropY + cropSize, name: "bottom-right" }
    ];

    for (let corner of corners) {
        if (Math.hypot(startX - corner.x, startY - corner.y) < 10) {
            isResizing = true;
            resizeCorner = corner.name;
            return;
        }
    }

    isDragging = true;
}

// เลื่อนหรือขยาย Crop
function handleMouseMove(event) {
        if (!isDragging && !isResizing) return;
    
        const rect = cropCanvas.getBoundingClientRect();
        const mouseX = event.clientX - rect.left;
        const mouseY = event.clientY - rect.top;
        const dx = mouseX - startX;
        const dy = mouseY - startY;
    
        if (isDragging) {
                cropX = Math.max(0, Math.min(startCropX + dx, cropCanvas.width - cropSize));
                cropY = Math.max(0, Math.min(startCropY + dy, cropCanvas.height - cropSize));
            } else if (isResizing) {
                switch (resizeCorner) {
                        case "top-left":
                                let newSizeTL = Math.max(50, startCropSize + Math.max(-dx, -dy));
                                let tempCropX_TL = startCropX + startCropSize - newSizeTL;
                                let tempCropY_TL = startCropY + startCropSize - newSizeTL;
                            
                                if (newSizeTL > startCropX + startCropSize || newSizeTL > startCropY + startCropSize) {
                                    newSizeTL = Math.min(startCropX + startCropSize, startCropY + startCropSize);
                                } else {
                                    cropX = Math.max(0, tempCropX_TL);
                                    cropY = Math.max(0, tempCropY_TL);
                                }
                            
                                cropSize = newSizeTL;
                                break;
                            
                            case "top-right":
                                let newSizeTR = Math.max(50, startCropSize + Math.max(dx, -dy));
                                let tempCropY_TR = startCropY + startCropSize - newSizeTR;
                            
                                if (newSizeTR > startCropY + startCropSize || cropX + newSizeTR > cropCanvas.width) {
                                    newSizeTR = Math.min(cropCanvas.width - cropX, startCropY + startCropSize);
                                } else {
                                    cropY = Math.max(0, tempCropY_TR);
                                }
                            
                                cropSize = newSizeTR;
                                break;
            
                        case "bottom-left":
                                let newSizeBL = Math.max(50, startCropSize + Math.max(-dx, dy));
                                let tempCropX = Math.min(startCropX + startCropSize - newSizeBL, cropCanvas.width - newSizeBL);
                                
                                if (newSizeBL > cropCanvas.height - cropY) {
                                        newSizeBL = cropCanvas.height - cropY; // จำกัดขนาดให้ไม่เกินขอบล่าง
                                } else {
                                        cropX = Math.max(0, tempCropX); // อัปเดตตำแหน่ง cropX เฉพาะเมื่อไม่เกินขอบ
                                }
                                
                                cropSize = Math.min(newSizeBL, cropCanvas.width - cropX, cropCanvas.height - cropY);
                                break;
            
                    case "bottom-right":
                        cropSize = Math.max(50, startCropSize + Math.max(dx, dy));
                        console.log("Good", cropSize, cropCanvas.width - cropX, cropCanvas.height - cropY)
                        cropSize = Math.min(cropSize, cropCanvas.width - cropX, cropCanvas.height - cropY);
                        break;
                }
            }
            
            
                    
    
        drawImage();
    }
    

// หยุดลาก / ขยาย
function stopInteraction() {
    isDragging = false;
    isResizing = false;
}

// บันทึกภาพและส่งไป Backend
async function saveCroppedImage() {
    const croppedCanvas = document.createElement("canvas");
    const croppedCtx = croppedCanvas.getContext("2d");

    croppedCanvas.width = 100;
    croppedCanvas.height = 100;

    croppedCtx.drawImage(cropCanvas, cropX, cropY, cropSize, cropSize, 0, 0, 100, 100);

    croppedCanvas.toBlob(async (blob) => {
        const formData = new FormData();
        formData.append("ProfilePic", blob, "cropped_profile.jpg");

        formData.append("Username", "");
        formData.append("FirstName", "");
        formData.append("LastName", "");
        formData.append("Bio", "");
        formData.append("Contact", "");
        formData.append("NewPassword", "");
        formData.append("ConfirmPassword", "");

        try {
            const response = await fetch("/user/edit", {
                method: "POST",
                body: formData,
            });

            const result = await response.json();

            if (response.ok) {
                alert("Profile updated successfully!");
                window.location.href = "/User/Edit";
            } else {
                alert(result.error || "Failed to update user information.");
            }
        } catch (error) {
            console.error("Error:", error);
            alert("An error occurred. Please try again.");
        }

        closeCropPopup();
    }, "image/jpeg");
}


let isMouseDownInside = false;

function onMouseDown(event) {
    const cropPopup = document.getElementById("cropContainer");

    // ถ้ากดเมาส์ที่ popup ให้บันทึกสถานะ
    isMouseDownInside = cropPopup.contains(event.target);
    
}

function onMouseUp(event) {
    const cropPopup = document.getElementById("cropPopup");

    if (!isMouseDownInside) {
        cropPopup.style.display = "none";
    }

    // รีเซ็ตสถานะ
    isMouseDownInside = false;
}


document.addEventListener("DOMContentLoaded", function () {
        document.getElementById("profilePicInput").addEventListener("change", openCropPopup);
        document.getElementById("cropCanvas").addEventListener("mousedown", startInteraction);
        document.addEventListener("mousemove", handleMouseMove);
        document.addEventListener("mouseup", stopInteraction);
        document.addEventListener("mousedown", onMouseDown);
        document.addEventListener("mouseup", onMouseUp);
        document.getElementById("cropContainer").onclick = (e) => e.stopPropagation();
});

