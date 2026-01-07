"use client";
import React from "react";

const DisplayDate = () => {
  const currentDate = new Date();

  return (
    <div>
      <p>
        {currentDate.toLocaleDateString("en-GB", {
          day: "2-digit",
          month: "2-digit",
          year: "numeric",
        })}
      </p>
    </div>
  );
};

export default DisplayDate;
