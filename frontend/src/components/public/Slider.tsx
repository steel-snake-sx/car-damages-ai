import { useEffect, useState } from 'react'

type Slide = {
  title: string
  subtitle: string
  image: string
}

type SliderProps = {
  slides: Slide[]
}

export function Slider({ slides }: SliderProps) {
  const [current, setCurrent] = useState(0)

  useEffect(() => {
    const timer = window.setInterval(() => {
      setCurrent((prev) => (prev + 1) % slides.length)
    }, 6000)

    return () => {
      window.clearInterval(timer)
    }
  }, [slides.length])

  const goTo = (index: number) => setCurrent(index)
  const move = (direction: -1 | 1) => setCurrent((prev) => (prev + direction + slides.length) % slides.length)

  return (
    <section className="slider-section" id="works">
      <div className="container">
        <div className="slider-wrapper" id="slider">
          {slides.map((slide, index) => (
            <div
              key={slide.title}
              className={index === current ? 'slide active' : 'slide'}
              style={{ backgroundImage: `url(${slide.image})` }}
            >
              <div className="slide-text">
                <h2>{slide.title}</h2>
                <p>{slide.subtitle}</p>
              </div>
            </div>
          ))}

          <div className="slider-nav">
            <div className="nav-btn" onClick={() => move(-1)}>
              ❮
            </div>
            <div className="nav-btn" onClick={() => move(1)}>
              ❯
            </div>
          </div>
          <div className="slider-dots" id="dots">
            {slides.map((slide, index) => (
              <button
                key={slide.title}
                className={index === current ? 'dot active' : 'dot'}
                onClick={() => goTo(index)}
                type="button"
              />
            ))}
          </div>
        </div>
      </div>
    </section>
  )
}
